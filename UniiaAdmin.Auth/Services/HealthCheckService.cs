namespace UniiaAdmin.Auth.Services;

using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Models;

public class HealthCheckService : IHealthCheckService
{
	private readonly IAdminUnitOfWork _adminUnitOfWork;
	private readonly IMongoUnitOfWork _mongoUnitOfWork;

	public HealthCheckService(
		IAdminUnitOfWork adminUnitOfWork,
		IMongoUnitOfWork mongoUnitOfWork)
	{
		_adminUnitOfWork = adminUnitOfWork;
		_mongoUnitOfWork = mongoUnitOfWork;
	}

	public async Task<bool> CanMongoConnectAsync() => await _mongoUnitOfWork.CanConnectAsync();

	public async Task<bool> CanAdminConnectAsync() => await _adminUnitOfWork.CanConnectAsync();
}
