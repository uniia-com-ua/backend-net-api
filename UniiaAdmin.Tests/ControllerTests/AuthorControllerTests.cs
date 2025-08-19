namespace UniiaAdmin.WebApi.Tests.ControllerTests;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Localization;
using Moq;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using UNIIAadminAPI.Controllers;
using Xunit;

public class AuthorControllerTests
{
	private readonly ControllerWebAppFactory<AuthorController> _factory;
	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public AuthorControllerTests()
	{
		var mockProvider = new MockProvider();

		mockProvider.Mock<IPhotoRepository>();
		mockProvider.Mock<IPhotoProvider>();
		mockProvider.Mock<IApplicationUnitOfWork>();
		mockProvider.Mock<MongoDbContext>();

		var localizer = mockProvider.Mock<IStringLocalizer<ErrorMessages>>();

		localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
			.Returns((string key, object[] args) => new LocalizedString(key, $"{key} {string.Join(", ", args)}"));

		_factory = new ControllerWebAppFactory<AuthorController>(mockProvider);
	}

	[Fact]
	public async Task GetAuthor_InvalidId_Returns404()
	{
		// Arrange
		const int invalidId = 9999;

		var mockedService = _factory.Mocks.Mock<IApplicationUnitOfWork>();
		mockedService.Setup(r => r.FindAsync<Author>(invalidId)).ReturnsAsync((Author)null!);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/authors/{invalidId}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetAuthor_ValidId_Returns200AndAuthor()
	{
		// Arrange
		const int validId = 1;

		var author = CreateTestAuthor();
		
		var mockedService = _factory.Mocks.Mock<IApplicationUnitOfWork>();
		mockedService.Setup(r => r.FindAsync<Author>(validId)).ReturnsAsync(author);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/authors/{validId}");
		var returnedAuthor = await DeserializeResponse<Author>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(
			JsonSerializer.Serialize(author, _jsonOptions),
			JsonSerializer.Serialize(returnedAuthor, _jsonOptions)
		);
	}

	[Fact]
	public async Task GetAuthorPhoto_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;
		var mockedService = _factory.Mocks.Mock<IPhotoProvider>();
		mockedService.Setup(r => r.GetPhotoAsync<Author, AuthorPhoto>(id)).ReturnsAsync(Result<AuthorPhoto>.Failure(new KeyNotFoundException("NotFound")));
		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/authors/{id}/photo");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetAuthorPhoto_Success_ReturnsFile()
	{
		// Arrange
		const int id = 1;
		var authorPhoto = new AuthorPhoto
		{
			File = [1, 2, 3],
		};

		var mockedService = _factory.Mocks.Mock<IPhotoProvider>();
		mockedService.Setup(r => r.GetPhotoAsync<Author, AuthorPhoto>(id)).ReturnsAsync(Result<AuthorPhoto>.Success(authorPhoto));
		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/authors/{id}/photo");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(MediaTypeNames.Image.Jpeg, response.Content.Headers.ContentType?.MediaType);
	}

	[Fact]
	public async Task GetAuthorPhoto_InvalidData_Returns404()
	{
		// Arrange
		const int id = 1;
		var mockedService = _factory.Mocks.Mock<IPhotoProvider>();
		mockedService.Setup(r => r.GetPhotoAsync<Author, AuthorPhoto>(id)).ReturnsAsync(Result<AuthorPhoto>.Failure(new InvalidDataException()));
		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/authors/{id}/photo");

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task GetPagedAuthors_Returns200WithList()
	{
		// Arrange
		var authors = new List<Author>
		{
			CreateTestAuthor(),
		};

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.GetPagedAsync<Author>(0, 10))
			.ReturnsAsync(authors);
		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/authors/page");
		var returned = await DeserializeResponse<List<Author>>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.NotNull(returned);
		Assert.Single(returned);
	}

	[Fact]
	public async Task CreateAuthor_Success_Returns200()
	{
		// Arrange
		var author = CreateTestAuthor();

		_factory.Mocks.Mock<IPhotoRepository>()
			.Setup(r => r.CreateAsync<Author, AuthorPhoto>(It.IsAny<Author>(), null))
			.ReturnsAsync(Result<AuthorPhoto>.SuccessNoContent());

		var client = _factory.CreateClient();

		var content = BuildMultipartContent(author);

		// Act
		var response = await client.PostAsync("/api/v1/authors", content);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task CreateAuthor_InvalidImage_Returns400()
	{
		// Arrange
		var author = CreateTestAuthor(1, "Old");

		_factory.Mocks.Mock<IPhotoRepository>()
			.Setup(r => r.CreateAsync<Author, AuthorPhoto>(It.IsAny<Author>(), It.IsAny<IFormFile>()))
			.ReturnsAsync(Result<AuthorPhoto>.Failure(new ArgumentException()));

		var client = _factory.CreateClient();

		var content = BuildMultipartContent(author);
		// Act
		var response = await client.PostAsync($"/api/v1/authors", content);

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task UpdateAuthor_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;
		var author = CreateTestAuthor();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Author>(id))
			.ReturnsAsync((Author)null!);

		var client = _factory.CreateClient();

		var content = BuildMultipartContent(author);
		// Act
		var response = await client.PatchAsync($"/api/v1/authors/{id}", content);

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task UpdateAuthor_InvalidImage_Returns400()
	{
		// Arrange
		const int id = 1;
		var newAuthor = CreateTestAuthor(1, "New");

		var author = CreateTestAuthor(1, "Old");	

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Author>(id))
			.ReturnsAsync(author);

		_factory.Mocks.Mock<IPhotoRepository>()
			.Setup(r => r.UpdateAsync<Author, AuthorPhoto>(It.IsAny<Author>(), It.IsAny<Author>(), It.IsAny<IFormFile>()))
			.ReturnsAsync(Result<AuthorPhoto>.Failure(new ArgumentException()));

		var client = _factory.CreateClient();

		var content = BuildMultipartContent(author);
		// Act
		var response = await client.PatchAsync($"/api/v1/authors/{id}", content);

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task UpdateAuthor_Success_Returns200()
	{
		// Arrange
		const int id = 1;
		var newAuthor = CreateTestAuthor(1, "New");

		var author = CreateTestAuthor(1, "Old");

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Author>(id))
			.ReturnsAsync(author);

		_factory.Mocks.Mock<IPhotoRepository>()
			.Setup(r => r.UpdateAsync<Author, AuthorPhoto>(It.IsAny<Author>(), It.IsAny<Author>(), null))
			.ReturnsAsync(Result<AuthorPhoto>.SuccessNoContent());

		var client = _factory.CreateClient();

		var content = BuildMultipartContent(newAuthor);

		// Act
		var response = await client.PatchAsync($"/api/v1/authors/{id}", content);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task DeleteAuthor_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Author>(id))
			.ReturnsAsync((Author)null!);

		var client = _factory.CreateClient();

		// Act
		var response = await client.DeleteAsync($"/api/v1/authors/{id}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task DeleteAuthor_Success_Returns200()
	{
		// Arrange
		const int id = 1;

		var author = CreateTestAuthor();

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Author>(id))
			.ReturnsAsync(author);

		_factory.Mocks.Mock<IPhotoRepository>()
			.Setup(r => r.DeleteAsync<Author, AuthorPhoto>(author))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();

		// Act
		var response = await client.DeleteAsync($"/api/v1/authors/{id}");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	private static Author CreateTestAuthor(int id = 1, string prefix = "Test")
	{
		return new Author
		{
			Id = id,
			FullName = $"{prefix} Author",
			Bio = $"{prefix} Bio",
			Email = $"{prefix.ToLower()}@example.com",
			OrcidId = $"{prefix} 0000-0000-0000-0000",
			ShortName = $"{prefix[0]}. Author",
			Url = $"https://example.com/{prefix.ToLower()}"
		};
	}

	private static MultipartFormDataContent BuildMultipartContent(Author author)
	{
		return new MultipartFormDataContent
	{
		{ new StringContent(author.FullName!), nameof(Author.FullName) },
		{ new StringContent(author.Bio!), nameof(Author.Bio) },
		{ new StringContent(author.Email!), nameof(Author.Email) },
		{ new StringContent(author.OrcidId!), nameof(Author.OrcidId) },
		{ new StringContent(author.ShortName!), nameof(Author.ShortName) },
		{ new StringContent(author.Url!), nameof(Author.Url) },
	};
	}
	private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
	{
		var json = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<T>(json, _jsonOptions);
	}

}
