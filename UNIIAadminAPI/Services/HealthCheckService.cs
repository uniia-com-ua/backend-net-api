namespace UniiaAdmin.WebApi.Services;

using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class HealthCheckService : IHealthCheckService
{
	private readonly IAdminUnitOfWork _adminUnitOfWork;
	private readonly IApplicationUnitOfWork _applicationUnitOfWork;
	private readonly IMongoUnitOfWork _mongoUnitOfWork;

	public HealthCheckService(
		IAdminUnitOfWork adminUnitOfWork,
		IApplicationUnitOfWork applicationUnitOfWork,
		IMongoUnitOfWork mongoUnitOfWork)
	{
		_adminUnitOfWork = adminUnitOfWork;
		_applicationUnitOfWork = applicationUnitOfWork;
		_mongoUnitOfWork = mongoUnitOfWork;
	}

	public HealthCheckComponent GetHealthStatusAsync(bool isHealthy)
	{
		return new HealthCheckComponent
		{
			Status = isHealthy ? "healthy" : "unhealthy",
			Timestamp = DateTime.UtcNow,
		};
	}

	public async Task<bool> CanMongoConnect() => await _mongoUnitOfWork.CanConnectAsync();

	public async Task<bool> CanAdminConnect() => await _adminUnitOfWork.CanConnectAsync();

	public async Task<bool> CanAppConnect() => await _applicationUnitOfWork.CanConnectAsync();
}
