namespace UniiaAdmin.WebApi.Tests.ControllerTests;

using Microsoft.Extensions.Localization;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using UniiaAdmin.Data.Controllers;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using Xunit;

public class PublicationTypeControllerTests
{
	private readonly ControllerWebAppFactory<PublicationTypeController> _factory;

	public PublicationTypeControllerTests()
	{
		var mockProvider = new MockProvider();

		mockProvider.Mock<IGenericRepository>();
		mockProvider.Mock<IApplicationUnitOfWork>();
		mockProvider.Mock<MongoDbContext>();

		var localizer = mockProvider.Mock<IStringLocalizer<ErrorMessages>>();
		localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
			.Returns((string key, object[] args) =>
				new LocalizedString(key, $"{key} {string.Join(", ", args)}"));

		_factory = new ControllerWebAppFactory<PublicationTypeController>(mockProvider);
	}

	[Fact]
	public async Task GetPublicationType_InvalidId_Returns404()
	{
		const int invalidId = 999;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationType>(invalidId))
			.ReturnsAsync((PublicationType)null!);

		var client = _factory.CreateClient();

		var response = await client.GetAsync($"/api/v1/publication-types/{invalidId}");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetPublicationType_ValidId_Returns200AndPublicationType()
	{
		const int validId = 1;
		var publicationType = new PublicationType { Id = validId, Name = "Journal" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationType>(validId))
			.ReturnsAsync(publicationType);

		var client = _factory.CreateClient();

		var response = await client.GetAsync($"/api/v1/publication-types/{validId}");
		var returned = await DeserializeResponse<PublicationType>(response);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(publicationType.Name, returned?.Name);
	}

	[Fact]
	public async Task GetPagedPublicationTypes_Returns200WithList()
	{
		var types = new List<PublicationType>
		{
			new PublicationType { Id = 1, Name = "Book" }
		};

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.GetPagedAsync<PublicationType>(0, 10))
			.ReturnsAsync(types);

		var client = _factory.CreateClient();

		var response = await client.GetAsync("/api/v1/publication-types/page");
		var returned = await DeserializeResponse<List<PublicationType>>(response);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!);
	}

	[Fact]
	public async Task CreatePublicationType_Success_Returns200()
	{
		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("Book"), Encoding.UTF8, "application/json");

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.CreateAsync(It.IsAny<PublicationType>()))
			.Returns(Task.CompletedTask);

		var response = await client.PostAsync("/api/v1/publication-types", content);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task UpdatePublicationType_NotFound_Returns404()
	{
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationType>(id))
			.ReturnsAsync((PublicationType)null!);

		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("NewType"), Encoding.UTF8, "application/json");

		var response = await client.PatchAsync($"/api/v1/publication-types/{id}", content);

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task UpdatePublicationType_Success_Returns200()
	{
		const int id = 1;
		var oldType = new PublicationType { Id = id, Name = "OldName" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationType>(id))
			.ReturnsAsync(oldType);

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.UpdateAsync(It.IsAny<PublicationType>(), oldType))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("NewName"), Encoding.UTF8, "application/json");

		var response = await client.PatchAsync($"/api/v1/publication-types/{id}", content);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task DeletePublicationType_NotFound_Returns404()
	{
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationType>(id))
			.ReturnsAsync((PublicationType)null!);

		var client = _factory.CreateClient();

		var response = await client.DeleteAsync($"/api/v1/publication-types/{id}");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task DeletePublicationType_Success_Returns200()
	{
		const int id = 1;
		var type = new PublicationType { Id = id, Name = "Conference" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<PublicationType>(id))
			.ReturnsAsync(type);

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.DeleteAsync(type))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();

		var response = await client.DeleteAsync($"/api/v1/publication-types/{id}");

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
