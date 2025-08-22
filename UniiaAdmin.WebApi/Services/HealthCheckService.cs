namespace UniiaAdmin.WebApi.Services;

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

	public async Task<bool> CanMongoConnectAsync() => await _mongoUnitOfWork.CanConnectAsync();

	public async Task<bool> CanAdminConnectAsync() => await _adminUnitOfWork.CanConnectAsync();

	public async Task<bool> CanAppConnectAsync() => await _applicationUnitOfWork.CanConnectAsync();
}
