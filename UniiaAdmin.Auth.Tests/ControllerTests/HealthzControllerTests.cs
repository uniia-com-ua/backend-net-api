using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System.Net;
using System.Text.Json;
using UniiaAdmin.Auth.Controllers;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Auth.Tests;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using Xunit;

namespace UniiaAdmin.Auth.Tests.ControllerTests
{
	public class HealthzControllerTests : WebApplicationFactory<HealthzController>
	{
		private readonly ControllerWebAppFactory<HealthzController> _factory;
		private readonly JsonSerializerOptions _jsonOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public HealthzControllerTests()
		{
			var mockProvider = new MockProvider();

			var healthService = mockProvider.Mock<IHealthCheckService>();

			healthService.Setup(c => c.CanMongoConnectAsync()).ReturnsAsync(true);
			healthService.Setup(c => c.CanAdminConnectAsync()).ReturnsAsync(true);

			_factory = new ControllerWebAppFactory<HealthzController>(mockProvider);
		}

		[Fact]
		public async Task Get_BasicHealthCheck_ReturnsHealthy()
		{
			// Arrange
			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync("/healthz");
			var json = await response.Content.ReadAsStringAsync();

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Contains("healthy", json);
		}

		[Fact]
		public async Task Ready_AllHealthy_ReturnsReady()
		{
			// Arrange
			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync("/healthz/ready");
			var json = await response.Content.ReadAsStringAsync();

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Contains("ready", json);
			Assert.Contains("postgresql", json);
			Assert.Contains("mongodb", json);
		}

		[Fact]
		public async Task Ready_Degraded_WhenAnyDbUnhealthy()
		{
			// Arrange
			_factory.Mocks.Mock<IHealthCheckService>()
				.Setup(c => c.CanMongoConnectAsync())
				.ReturnsAsync(false);

			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync("/healthz/ready");
			var json = await response.Content.ReadAsStringAsync();

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Contains("warning", json);
		}

		[Fact]
		public async Task Live_ReturnsAlive()
		{
			// Arrange
			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync("/healthz/live");
			var body = await DeserializeResponse<HealthCheckComponent>(response);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal("alive", body!.Status);
		}

		[Fact]
		public async Task Get_ReturnsVersionFromEnvironmentVariable()
		{
			// Arrange
			var expectedVersion = "1.2.3";
			Environment.SetEnvironmentVariable("APP_VERSION", expectedVersion);
			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync("/healthz/version");
			var json = await response.Content.ReadAsStringAsync();

			// Assert
			Assert.NotNull(json);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Contains(expectedVersion, json);
		}

		[Fact]
		public async Task Get_ReturnsUnknownWhenEnvironmentVariableNotSet()
		{
			// Arrange
			Environment.SetEnvironmentVariable("APP_VERSION", null);
			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync("/healthz/version");
			var json = await response.Content.ReadAsStringAsync();

			// Assert
			Assert.NotNull(json);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Contains("unknown", json);
		}

		private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
		{
			var json = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<T>(json, _jsonOptions);
		}
	}
}
