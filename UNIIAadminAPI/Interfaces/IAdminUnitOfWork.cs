namespace UniiaAdmin.WebApi.Interfaces;

using Microsoft.AspNetCore.Identity;

public interface IAdminUnitOfWork
{
	public IQueryable<IdentityRoleClaim<string>> RoleClaims();

	public Task<bool> CanConnectAsync();
}
