namespace UniiaAdmin.WebApi.Tests.ControllerTests;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Controllers;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using Xunit;

public class UniversityControllerTests
{
	private readonly ControllerWebAppFactory<UniversityController> _factory;

	public UniversityControllerTests()
	{
		var mockProvider = new MockProvider();

		mockProvider.Mock<ISmallPhotoRepository>();
		mockProvider.Mock<IPhotoProvider>();
		mockProvider.Mock<IApplicationUnitOfWork>();
		mockProvider.Mock<MongoDbContext>();

		var localizer = mockProvider.Mock<IStringLocalizer<ErrorMessages>>();
		localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
			.Returns((string key, object[] args) =>
				new LocalizedString(key, $"{key} {string.Join(", ", args)}"));

		_factory = new ControllerWebAppFactory<UniversityController>(mockProvider);
	}

	[Fact]
	public async Task GetUniversity_InvalidId_Returns404()
	{
		// Arrange
		const int invalidId = 9999;

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<University>(invalidId))
			.ReturnsAsync((University)null!);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/universities/{invalidId}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetUniversity_ValidId_Returns200AndUniversity()
	{
		// Arrange
		const int validId = 1;

		var university = CreateTestUniversity();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<University>(validId))
			.ReturnsAsync(university);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/universities/{validId}");
		var returned = await DeserializeResponse<University>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(
			JsonSerializer.Serialize(university, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
			JsonSerializer.Serialize(returned, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
		);
	}

	[Fact]
	public async Task GetUniversityPhoto_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;
		_factory.Mocks.Mock<IPhotoProvider>()
			.Setup(r => r.GetPhotoAsync<University, UniversityPhoto>(id))
			.ReturnsAsync(Result<UniversityPhoto>.Failure(new KeyNotFoundException("NotFound")));

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/universities/{id}/photo");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetUniversityPhoto_Success_ReturnsFile()
	{
		// Arrange
		const int id = 1;
		var universityPhoto = new UniversityPhoto { File = [1, 2, 3] };

		_factory.Mocks.Mock<IPhotoProvider>()
			.Setup(r => r.GetPhotoAsync<University, UniversityPhoto>(id))
			.ReturnsAsync(Result<UniversityPhoto>.Success(universityPhoto));

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/universities/{id}/photo");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(MediaTypeNames.Image.Jpeg, response.Content.Headers.ContentType?.MediaType);
	}

	[Fact]
	public async Task GetPagedUniversities_Returns200WithList()
	{
		// Arrange
		var universities = new PageData<University> { Items = new() { CreateTestUniversity() }, TotalCount = 1 };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.GetPagedAsync<University>(0, 10, null))
			.ReturnsAsync(universities);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/universities/page");
		var returned = await DeserializeResponse<PageData<University>>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!.Items);
	}

	[Fact]
	public async Task CreateUniversity_Success_Returns200()
	{
		// Arrange
		var university = CreateTestUniversity();

		_factory.Mocks.Mock<ISmallPhotoRepository>()
			.Setup(r => r.CreateAsync<University, UniversityPhoto>(It.IsAny<University>(), null, null))
			.ReturnsAsync(Result<UniversityPhoto>.SuccessNoContent());

		var client = _factory.CreateClient();

		var content = BuildMultipartContent(university);

		// Act
		var response = await client.PostAsync("/api/v1/universities", content);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task DeleteUniversity_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<University>(id))
			.ReturnsAsync((University)null!);

		var client = _factory.CreateClient();

		// Act
		var response = await client.DeleteAsync($"/api/v1/universities/{id}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task DeleteUniversity_Success_Returns200()
	{
		// Arrange
		const int id = 1;
		var university = CreateTestUniversity();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<University>(id))
			.ReturnsAsync(university);

		_factory.Mocks.Mock<ISmallPhotoRepository>()
			.Setup(r => r.DeleteAsync<University, UniversityPhoto>(university))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();

		// Act
		var response = await client.DeleteAsync($"/api/v1/universities/{id}");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	private static University CreateTestUniversity(int id = 1, string prefix = "Test")
	{
		return new University
		{
			Id = id,
			Name = $"{prefix} University",
			ShortName = $"{prefix} Description",
		};
	}

	private static MultipartFormDataContent BuildMultipartContent(University university)
	{
		return new MultipartFormDataContent
		{
			{ new StringContent(university.Name!), nameof(University.Name) },
			{ new StringContent(university.ShortName!), nameof(University.ShortName) },
		};
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
