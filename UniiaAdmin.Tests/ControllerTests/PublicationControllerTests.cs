namespace UniiaAdmin.WebApi.Tests.ControllerTests;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using UNIIAadminAPI.Controllers;
using Xunit;

public class PublicationControllerTests
{
	private readonly ControllerWebAppFactory<PublicationController> _factory;

	public PublicationControllerTests()
	{
		var mockProvider = new MockProvider();

		mockProvider.Mock<IFileRepository>();
		mockProvider.Mock<IApplicationUnitOfWork>();
		mockProvider.Mock<MongoDbContext>();

		var localizer = mockProvider.Mock<IStringLocalizer<ErrorMessages>>();
		localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
			.Returns((string key, object[] args) => new LocalizedString(key, $"{key} {string.Join(", ", args)}"));

		_factory = new ControllerWebAppFactory<PublicationController>(mockProvider);
	}

	[Fact]
	public async Task GetPublication_InvalidId_Returns404()
	{
		// Arrange
		var client = _factory.CreateClient();
		const int invalidId = 9999;

		// Act
		var response = await client.GetAsync($"/api/v1/publications/{invalidId}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetPublication_ValidId_Returns200()
	{
		// Arrange
		const int validId = 1;
		var publication = CreateTestPublication();

		var mockedRepo = _factory.Mocks.Mock<IApplicationUnitOfWork>();
		mockedRepo.Setup(r => r.GetByIdWithIncludesAsync(
				It.IsAny<Expression<Func<Publication, bool>>>(),
				It.IsAny<Expression<Func<Publication, object>>[]>()
			)).ReturnsAsync(publication);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/publications/{validId}");
		var returned = await DeserializeResponse<Publication>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(
			JsonSerializer.Serialize(publication, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
			JsonSerializer.Serialize(returned, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
		);
	}

	[Fact]
	public async Task GetPublicationFile_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;
		var mockedRepo = _factory.Mocks.Mock<IFileRepository>();
		mockedRepo.Setup(r => r.GetFileAsync<Publication, PublicationFile>(id))
			.ReturnsAsync(Result<PublicationFile>.Failure(new KeyNotFoundException("Not found")));

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/publications/{id}/file");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetPublicationFile_BadRequest_Returns400()
	{
		// Arrange
		const int id = 1;
		var mockedRepo = _factory.Mocks.Mock<IFileRepository>();
		mockedRepo.Setup(r => r.GetFileAsync<Publication, PublicationFile>(id))
			.ReturnsAsync(Result<PublicationFile>.Failure(new InvalidDataException("Not found")));

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/publications/{id}/file");

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task GetPublicationFile_Success_ReturnsPdf()
	{
		// Arrange
		const int id = 1;
		var file = new PublicationFile { File = [1, 2, 3] };

		var mockedRepo = _factory.Mocks.Mock<IFileRepository>();
		mockedRepo.Setup(r => r.GetFileAsync<Publication, PublicationFile>(id))
			.ReturnsAsync(Result<PublicationFile>.Success(file));

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/publications/{id}/file");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(MediaTypeNames.Application.Pdf, response.Content.Headers.ContentType?.MediaType);
	}

	[Fact]
	public async Task GetPagedPublications_Returns200WithList()
	{
		// Arrange
		var list = new List<Publication> { CreateTestPublication() };
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.GetPagedAsync<Publication>(0, 10))
			.ReturnsAsync(list);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/publications/page");
		var returned = await DeserializeResponse<List<Publication>>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!);
	}

	[Fact]
	public async Task CreatePublication_TypeNotFound_Returns404()
	{
		// Arrange
		var publication = CreateTestPublication();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationType>(It.IsAny<int>()))
			.ReturnsAsync(false);

		var client = _factory.CreateClient();
		var content = BuildMultipartContent(publication);

		// Act
		var response = await client.PostAsync($"/api/v1/publications", content);

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task CreatePublication_LangNotFound_Returns404()
	{
		// Arrange
		var publication = CreateTestPublication();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationType>(It.IsAny<int>()))
			.ReturnsAsync(true);

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationLanguage>(It.IsAny<int>()))
			.ReturnsAsync(false);

		var client = _factory.CreateClient();
		var content = BuildMultipartContent(publication);

		// Act
		var response = await client.PostAsync($"/api/v1/publications", content);

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task CreatePublication_Success_Returns200()
	{
		// Arrange
		var publication = CreateTestPublication();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationType>(It.IsAny<int>()))
			.ReturnsAsync(true);

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationLanguage>(It.IsAny<int>()))
			.ReturnsAsync(true);

		_factory.Mocks.Mock<IFileRepository>()
			.Setup(r => r.CreateAsync<Publication, PublicationFile>(It.IsAny<Publication>(), It.IsAny<IFormFile>()))
			.ReturnsAsync(Result<PublicationFile>.SuccessNoContent());

		var client = _factory.CreateClient();
		var content = BuildMultipartContent(publication);

		// Act
		var response = await client.PostAsync("/api/v1/publications", content);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task UpdatePublication_Success_Returns200()
	{
		// Arrange
		var existedPublication = CreateTestPublication(1);
		var publication = CreateTestPublication(2, "New");

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationType>(It.IsAny<int>()))
			.ReturnsAsync(true);

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationLanguage>(It.IsAny<int>()))
			.ReturnsAsync(true);

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Publication>(It.IsAny<int>()))
			.ReturnsAsync(existedPublication);

		_factory.Mocks.Mock<IFileRepository>()
			.Setup(r => r.UpdateAsync<Publication, PublicationFile>(It.IsAny<Publication>(), It.IsAny<Publication>(), It.IsAny<IFormFile>()))
			.ReturnsAsync(Result<PublicationFile>.SuccessNoContent());

		var client = _factory.CreateClient();
		var existedContent = BuildMultipartContent(publication);
		var content = BuildMultipartContent(existedPublication);

		existedContent.Add(content);

		// Act
		var response = await client.PatchAsync($"/api/v1/publications/{existedPublication.Id}", existedContent);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task UpdatePublication_BadRequest_Returns400()
	{
		// Arrange
		var existedPublication = CreateTestPublication(1);
		var publication = CreateTestPublication(2, "New");

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationType>(It.IsAny<int>()))
			.ReturnsAsync(true);

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationLanguage>(It.IsAny<int>()))
			.ReturnsAsync(true);

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Publication>(It.IsAny<int>()))
			.ReturnsAsync(existedPublication);

		_factory.Mocks.Mock<IFileRepository>()
			.Setup(r => r.UpdateAsync<Publication, PublicationFile>(It.IsAny<Publication>(), It.IsAny<Publication>(), It.IsAny<IFormFile>()))
			.ReturnsAsync(Result<PublicationFile>.Failure(new ArgumentException()));

		var client = _factory.CreateClient();
		var existedContent = BuildMultipartContent(publication);
		var content = BuildMultipartContent(existedPublication);

		existedContent.Add(content);

		// Act
		var response = await client.PatchAsync($"/api/v1/publications/{existedPublication.Id}", existedContent);

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task UpdatePublication_PublNotFound_Returns404()
	{
		// Arrange
		var publication = CreateTestPublication();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Publication>(It.IsAny<int>()))
			.ReturnsAsync((Publication)null!);

		var client = _factory.CreateClient();
		var content = BuildMultipartContent(publication);

		// Act
		var response = await client.PatchAsync($"/api/v1/publications/{publication.Id}", content);

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task UpdatePublication_TypeNotFound_Returns404()
	{
		// Arrange
		var publication = CreateTestPublication();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Publication>(It.IsAny<int>()))
			.ReturnsAsync(publication);

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationType>(It.IsAny<int>()))
			.ReturnsAsync(false);

		var client = _factory.CreateClient();
		var content = BuildMultipartContent(publication);

		// Act
		var response = await client.PatchAsync($"/api/v1/publications/{publication.Id}", content);

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task UpdatePublication_LangNotFound_Returns404()
	{
		// Arrange
		var publication = CreateTestPublication();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Publication>(It.IsAny<int>()))
			.ReturnsAsync(publication);

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationType>(It.IsAny<int>()))
			.ReturnsAsync(true);

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.AnyAsync<PublicationLanguage>(It.IsAny<int>()))
			.ReturnsAsync(false);

		var client = _factory.CreateClient();
		var content = BuildMultipartContent(publication);

		// Act
		var response = await client.PatchAsync($"/api/v1/publications/{publication.Id}", content);

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task DeletePublication_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Publication>(id))
			.ReturnsAsync((Publication)null!);

		var client = _factory.CreateClient();

		// Act
		var response = await client.DeleteAsync($"/api/v1/publications/{id}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task DeletePublication_Success_Returns200()
	{
		// Arrange
		const int id = 1;
		var publication = CreateTestPublication();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Publication>(id))
			.ReturnsAsync(publication);

		_factory.Mocks.Mock<IFileRepository>()
			.Setup(r => r.DeleteAsync<Publication, PublicationFile>(publication))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();

		// Act
		var response = await client.DeleteAsync($"/api/v1/publications/{id}");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	private static Publication CreateTestPublication(int id = 1, string prefix = "Test")
	{
		return new Publication
		{
			Id = id,
			Title = $"{prefix} Publication",
			DOI = $"{prefix}-DOI-{id}",
			Annotation = $"{prefix} Annotation",
			AnnotationFormat = "text/plain",
			ISBN = $"{prefix}-ISBN-{id}",
			LicenseURL = $"{prefix}-License-{id}",
			Publisher = $"{prefix} Publisher",
			Status = PublicationStatus.Published,
			Pages = 100,
			PublicationYear = 2023,
			URL = $"{prefix}-URL-{id}",
			CreatedDate = DateTime.UtcNow,
			LastModifiedDate = DateTime.UtcNow
		};
	}

	private static MultipartFormDataContent BuildMultipartContent(Publication publication)
	{
		var content = new MultipartFormDataContent
		{
			{ new StringContent(publication.Title ?? string.Empty), nameof(Publication.Title) },
			{ new StringContent(publication.DOI ?? string.Empty), nameof(Publication.DOI) },
			{ new StringContent(publication.Annotation ?? string.Empty), nameof(Publication.Annotation) },
			{ new StringContent(publication.AnnotationFormat ?? string.Empty), nameof(Publication.AnnotationFormat) },
			{ new StringContent(publication.ISBN ?? string.Empty), nameof(Publication.ISBN) },
			{ new StringContent(publication.LicenseURL ?? string.Empty), nameof(Publication.LicenseURL) },
			{ new StringContent(publication.Publisher ?? string.Empty), nameof(Publication.Publisher) },
			{ new StringContent(publication.Status.ToString()), nameof(Publication.Status) },
			{ new StringContent(publication.Pages.ToString()), nameof(Publication.Pages) },
			{ new StringContent(publication.PublicationYear.ToString()), nameof(Publication.PublicationYear) },
			{ new StringContent(publication.URL ?? string.Empty), nameof(Publication.URL) },
			{ new StringContent(publication.CreatedDate.ToString("o")), nameof(Publication.CreatedDate) },
			{ new StringContent(publication.LastModifiedDate.ToString("o")), nameof(Publication.LastModifiedDate) }
		};

		return content;
	}


	private static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
	{
		var json = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});
	}
}
