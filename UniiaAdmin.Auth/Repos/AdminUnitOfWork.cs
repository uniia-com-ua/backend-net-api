namespace UniiaAdmin.Auth.Repos;

using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Data;

public class AdminUnitOfWork : IAdminUnitOfWork
{
	private readonly AdminContext _adminContext;

	public AdminUnitOfWork(AdminContext adminContext)
	{
		_adminContext = adminContext;
	}

	public async Task<bool> CreateAsync() => await _adminContext.Database.EnsureCreatedAsync();

	public async Task<bool> CanConnectAsync() => await _adminContext.Database.CanConnectAsync();
}
