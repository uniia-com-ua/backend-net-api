using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using UniiaAdmin.Data.Models;
using UniiaAdmin.Tests;
using UniiaAdmin.WebApi.Controllers;
using UniiaAdmin.WebApi.Interfaces;
using Xunit;

namespace UniiaAdmin.Tests.ControllerTests;

public class LogActionControllerTests
{
	private readonly ControllerWebAppFactory<LogActionController> _factory;

	public LogActionControllerTests()
	{
		var mockProvider = new MockProvider();
		mockProvider.Mock<ILogPaginationService>();

		_factory = new ControllerWebAppFactory<LogActionController>(mockProvider);
	}

	[Fact]
	public async Task GetPagedLogs_ReturnsOkWithLogs()
	{
		// Arrange
		var logs = new List<LogActionModel>
			{
				new() { Id = new ObjectId(), ModelId = 1, ModelName = "Test", UserId = "user1" }
			};

		var paginationMock = _factory.Mocks.Mock<ILogPaginationService>();
		paginationMock.Setup(p => p.GetPagedListAsync(0, 10)).ReturnsAsync(logs);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/log-actions/page");
		var returned = await DeserializeResponse<List<LogActionModel>>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!);
	}

	[Fact]
	public async Task GetLogByModelId_ReturnsOkWithFilteredLogs()
	{
		// Arrange
		var logs = new List<LogActionModel>
			{
				new() { Id = new ObjectId(), ModelId = 1, ModelName = "Test", UserId = "user1" }
			};

		var paginationMock = _factory.Mocks.Mock<ILogPaginationService>();
		paginationMock.Setup(p => p.GetPagedListAsync(1, "Test", 0, 10)).ReturnsAsync(logs);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/log-actions/model/1?modelName=Test");
		var returned = await DeserializeResponse<List<LogActionModel>>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!);
		Assert.All(returned!, x => Assert.Equal(1, x.ModelId));
		Assert.All(returned!, x => Assert.Equal("Test", x.ModelName));
	}

	[Fact]
	public async Task GetByUserId_ReturnsOkWithUserLogs()
	{
		// Arrange
		var logs = new List<LogActionModel>
			{
				new() { Id = new ObjectId(), ModelId = 1, ModelName = "Test", UserId = "user1" }
			};

		var paginationMock = _factory.Mocks.Mock<ILogPaginationService>();
		paginationMock.Setup(p => p.GetPagedListAsync("user1", 0, 10)).ReturnsAsync(logs);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/log-actions/user/user1");
		var returned = await DeserializeResponse<List<LogActionModel>>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!);
		Assert.All(returned!, x => Assert.Equal("user1", x.UserId));
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
