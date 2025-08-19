namespace UniiaAdmin.WebApi.Tests.ControllerTests;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using UNIIAadminAPI.Controllers;
using Xunit;

public class PublicationLanguageControllerTests
{
	private readonly ControllerWebAppFactory<PublicationLanguageController> _factory;

	public PublicationLanguageControllerTests()
	{
		var mockProvider = new MockProvider();

		mockProvider.Mock<IGenericRepository>();
		mockProvider.Mock<IApplicationUnitOfWork>();
		mockProvider.Mock<MongoDbContext>();

		var localizer = mockProvider.Mock<IStringLocalizer<ErrorMessages>>();
		localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
			.Returns((string key, object[] args) =>
				new LocalizedString(key, $"{key} {string.Join(", ", args)}"));

		_factory = new ControllerWebAppFactory<PublicationLanguageController>(mockProvider);
	}

	[Fact]
	public async Task Get_InvalidId_Returns404()
	{
		const int invalidId = 999;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationLanguage>(invalidId))
			.ReturnsAsync((PublicationLanguage)null!);

		var client = _factory.CreateClient();

		var response = await client.GetAsync($"/api/v1/publication-languages/{invalidId}");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task Get_ValidId_Returns200AndLanguage()
	{
		const int validId = 1;
		var language = new PublicationLanguage { Id = validId, Name = "English" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationLanguage>(validId))
			.ReturnsAsync(language);

		var client = _factory.CreateClient();

		var response = await client.GetAsync($"/api/v1/publication-languages/{validId}");
		var returned = await DeserializeResponse<PublicationLanguage>(response);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(language.Name, returned?.Name);
	}

	[Fact]
	public async Task GetPaginated_Returns200WithList()
	{
		var languages = new List<PublicationLanguage>
		{
			new PublicationLanguage { Id = 1, Name = "English" }
		};

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.GetPagedAsync<PublicationLanguage>(0, 10))
			.ReturnsAsync(languages);

		var client = _factory.CreateClient();

		var response = await client.GetAsync("/api/v1/publication-languages/page");
		var returned = await DeserializeResponse<List<PublicationLanguage>>(response);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!);
	}

	[Fact]
	public async Task CreateLanguage_Success_Returns200()
	{
		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("English"), Encoding.UTF8, "application/json");

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.CreateAsync(It.IsAny<PublicationLanguage>()))
			.Returns(Task.CompletedTask);

		var response = await client.PostAsync("/api/v1/publication-languages", content);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task UpdateLanguage_NotFound_Returns404()
	{
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationLanguage>(id))
			.ReturnsAsync((PublicationLanguage)null!);

		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("NewLang"), Encoding.UTF8, "application/json");

		var response = await client.PatchAsync($"/api/v1/publication-languages/{id}", content);

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task UpdateLanguage_Success_Returns200()
	{
		const int id = 1;
		var oldLanguage = new PublicationLanguage { Id = id, Name = "OldLang" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationLanguage>(id))
			.ReturnsAsync(oldLanguage);

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.UpdateAsync(It.IsAny<PublicationLanguage>(), oldLanguage))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("NewLang"), Encoding.UTF8, "application/json");

		var response = await client.PatchAsync($"/api/v1/publication-languages/{id}", content);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task DeleteLanguage_NotFound_Returns404()
	{
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationLanguage>(id))
			.ReturnsAsync((PublicationLanguage)null!);

		var client = _factory.CreateClient();

		var response = await client.DeleteAsync($"/api/v1/publication-languages/{id}");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task DeleteLanguage_Success_Returns200()
	{
		const int id = 1;
		var language = new PublicationLanguage { Id = id, Name = "English" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationLanguage>(id))
			.ReturnsAsync(language);

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.DeleteAsync(language))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();

		var response = await client.DeleteAsync($"/api/v1/publication-languages/{id}");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
