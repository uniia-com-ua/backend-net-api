using Moq;
using System;
using System.Threading.Tasks;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Services;
using Xunit;

namespace UniiaAdmin.WebApi.Tests.ServiceTests;

public class HealthCheckServiceTests
{
	private readonly Mock<IAdminUnitOfWork> _adminMock;
	private readonly Mock<IApplicationUnitOfWork> _appMock;
	private readonly Mock<IMongoUnitOfWork> _mongoMock;
	private readonly HealthCheckService _service;

	public HealthCheckServiceTests()
	{
		_adminMock = new Mock<IAdminUnitOfWork>();
		_appMock = new Mock<IApplicationUnitOfWork>();
		_mongoMock = new Mock<IMongoUnitOfWork>();

		_service = new HealthCheckService(
			_adminMock.Object,
			_appMock.Object,
			_mongoMock.Object);
	}

	[Fact]
	public void GetHealthStatusAsync_WhenHealthy_ReturnsHealthyStatus()
	{
		// Act
		var result = _service.GetHealthStatusAsync(true);

		// Assert
		Assert.Equal("healthy", result.Status);
		Assert.True((DateTime.UtcNow - result.Timestamp).TotalSeconds < 1);
	}

	[Fact]
	public void GetHealthStatusAsync_WhenUnhealthy_ReturnsUnhealthyStatus()
	{
		// Act
		var result = _service.GetHealthStatusAsync(false);

		// Assert
		Assert.Equal("unhealthy", result.Status);
		Assert.True((DateTime.UtcNow - result.Timestamp).TotalSeconds < 1);
	}

	[Fact]
	public async Task CanMongoConnect_ReturnsTrue_WhenMongoCanConnect()
	{
		// Arrange
		_mongoMock.Setup(m => m.CanConnectAsync()).ReturnsAsync(true);

		// Act
		var result = await _service.CanMongoConnectAsync();

		// Assert
		Assert.True(result);
	}

	[Fact]
	public async Task CanMongoConnect_ReturnsFalse_WhenMongoCannotConnect()
	{
		_mongoMock.Setup(m => m.CanConnectAsync()).ReturnsAsync(false);

		var result = await _service.CanMongoConnectAsync();

		Assert.False(result);
	}

	[Fact]
	public async Task CanAdminConnect_ReturnsTrue_WhenAdminCanConnect()
	{
		_adminMock.Setup(a => a.CanConnectAsync()).ReturnsAsync(true);

		var result = await _service.CanAdminConnectAsync();

		Assert.True(result);
	}

	[Fact]
	public async Task CanAppConnect_ReturnsTrue_WhenAppCanConnect()
	{
		_appMock.Setup(a => a.CanConnectAsync()).ReturnsAsync(true);

		var result = await _service.CanAppConnectAsync();

		Assert.True(result);
	}
}
