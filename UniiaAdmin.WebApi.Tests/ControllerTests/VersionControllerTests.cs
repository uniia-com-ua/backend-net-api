using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UniiaAdmin.WebApi.Controllers;
using Xunit;

namespace UniiaAdmin.WebApi.Tests.ControllerTests
{
	public class VersionControllerTests
	{	
		[Fact]
		public void Get_ReturnsVersionFromEnvironmentVariable()
		{
			// Arrange
			var expectedVersion = "1.2.3";
			Environment.SetEnvironmentVariable("APP_VERSION", expectedVersion);
			var controller = new VersionController();

			// Act
			var result = controller.Get() as OkObjectResult;
			var json = JsonSerializer.Serialize(result?.Value);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(StatusCodes.Status200OK, result!.StatusCode);
			Assert.Contains(expectedVersion, json);
		}

		[Fact]
		public void Get_ReturnsUnknownWhenEnvironmentVariableNotSet()
		{
			// Arrange
			Environment.SetEnvironmentVariable("APP_VERSION", null);
			var controller = new VersionController();

			// Act
			var result = controller.Get() as OkObjectResult;
			var json = JsonSerializer.Serialize(result?.Value);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(StatusCodes.Status200OK, result!.StatusCode);
			Assert.Contains("unknown", json);
		}
	}
}
