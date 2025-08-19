using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System.Net;
using System.Text.Json;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UNIIAadminAPI.Controllers;
using Xunit;

namespace UniiaAdmin.WebApi.Tests.ControllerTests
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

			healthService.Setup(c => c.CanAppConnect()).ReturnsAsync(true);
			healthService.Setup(c => c.CanMongoConnect()).ReturnsAsync(true);
			healthService.Setup(c => c.CanAdminConnect()).ReturnsAsync(true);

			healthService.Setup(s => s.GetHealthStatusAsync(It.IsAny<bool>()))
						 .Returns<bool>(ok => new HealthCheckComponent
						 {
							 Status = ok ? "healthy" : "unhealthy",
							 Timestamp = DateTime.UtcNow
						 });

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
			Assert.Contains("admin_postgresql", json);
			Assert.Contains("application_postgresql", json);
			Assert.Contains("mongodb", json);
		}

		[Fact]
		public async Task Ready_Degraded_WhenAnyDbUnhealthy()
		{
			// Arrange
			_factory.Mocks.Mock<IHealthCheckService>()
				.Setup(c => c.CanMongoConnect())
				.ReturnsAsync(false);

			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync("/healthz/ready");
			var json = await response.Content.ReadAsStringAsync();

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Contains("degraded", json);
			Assert.Contains("unhealthy", json);
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
		public async Task Components_ReturnsComponentStatuses()
		{
			// Arrange
			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync("/healthz/components");
			var json = await response.Content.ReadAsStringAsync();

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Contains("components_checked", json);
			Assert.Contains("admin_db", json);
			Assert.Contains("application_db", json);
			Assert.Contains("mongodb", json);
		}


		private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
		{
			var json = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<T>(json, _jsonOptions);
		}
	}
}
