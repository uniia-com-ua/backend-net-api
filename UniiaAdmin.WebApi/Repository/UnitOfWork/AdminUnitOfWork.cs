namespace UniiaAdmin.WebApi.Repository.UnitOfWork;

using Microsoft.AspNetCore.Identity;
using System.Linq;
using UniiaAdmin.Data.Data;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class AdminUnitOfWork : IAdminUnitOfWork
{
	private readonly AdminContext _adminContext;

	public AdminUnitOfWork(AdminContext adminContext)
	{
		_adminContext = adminContext;
	}

	public IQueryable<IdentityRoleClaim<string>> RoleClaims()
	=> _adminContext.RoleClaims;

	public async Task<bool> CanConnectAsync()
		=> await _adminContext.Database.CanConnectAsync();
}
