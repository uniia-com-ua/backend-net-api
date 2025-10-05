namespace UniiaAdmin.WebApi.Tests.ControllerTests;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Localization;
using Moq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Controllers;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using Xunit;

public class KeywordControllerTests
{
	private readonly ControllerWebAppFactory<KeywordController> _factory;

	public KeywordControllerTests()
	{
		var mockProvider = new MockProvider();

		mockProvider.Mock<IGenericRepository>();
		mockProvider.Mock<IApplicationUnitOfWork>();
		mockProvider.Mock<MongoDbContext>();

		var localizer = mockProvider.Mock<IStringLocalizer<ErrorMessages>>();

		localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
			.Returns((string key, object[] args) => new LocalizedString(key, $"{key} {string.Join(", ", args)}"));

		_factory = new ControllerWebAppFactory<KeywordController>(mockProvider);
	}

	[Fact]
	public async Task GetKeyword_InvalidId_Returns404()
	{
		// Arrange
		const int invalidId = 999;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Keyword>(invalidId))
			.ReturnsAsync((Keyword)null!);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/keywords/{invalidId}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetKeyword_ValidId_Returns200AndKeyword()
	{
		// Arrange
		const int validId = 1;
		var keyword = new Keyword { Id = validId, Word = "AI" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Keyword>(validId))
			.ReturnsAsync(keyword);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/keywords/{validId}");
		var returned = await DeserializeResponse<Keyword>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(keyword.Word, returned?.Word);
	}

	[Fact]
	public async Task GetPagedKeywords_Returns200WithList()
	{
		// Arrange
		var keywords = new PageData<Keyword> { Items = new() { new() { Id = 1, Word = "AI" } }, TotalCount = 1 };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.GetPagedAsync<Keyword>(0, 10, null))
			.ReturnsAsync(keywords);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/keywords/page");
		var returned = await DeserializeResponse<PageData<Keyword>>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!.Items);
	}

	[Fact]
	public async Task CreateKeyword_Success_Returns200()
	{
		// Arrange
		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("AI"), Encoding.UTF8, "application/json");

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.CreateAsync(It.IsAny<Keyword>()))
			.Returns(Task.CompletedTask);

		// Act
		var response = await client.PostAsync("/api/v1/keywords", content);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task UpdateKeyword_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Keyword>(id))
			.ReturnsAsync((Keyword)null!);

		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("NewWord"), Encoding.UTF8, "application/json");

		// Act
		var response = await client.PatchAsync($"/api/v1/keywords/{id}", content);

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task UpdateKeyword_Success_Returns200()
	{
		// Arrange
		const int id = 1;
		var oldKeyword = new Keyword { Id = id, Word = "OldWord" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Keyword>(id))
			.ReturnsAsync(oldKeyword);

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.UpdateAsync(It.IsAny<Keyword>(), oldKeyword))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("NewWord"), Encoding.UTF8, "application/json");

		// Act
		var response = await client.PatchAsync($"/api/v1/keywords/{id}", content);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task DeleteKeyword_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Keyword>(id))
			.ReturnsAsync((Keyword)null!);

		var client = _factory.CreateClient();

		// Act
		var response = await client.DeleteAsync($"/api/v1/keywords/{id}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task DeleteKeyword_Success_Returns200()
	{
		// Arrange
		const int id = 1;
		var keyword = new Keyword { Id = id, Word = "AI" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Keyword>(id))
			.ReturnsAsync(keyword);

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.DeleteAsync(keyword))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();

		// Act
		var response = await client.DeleteAsync($"/api/v1/keywords/{id}");

		// Assert
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
