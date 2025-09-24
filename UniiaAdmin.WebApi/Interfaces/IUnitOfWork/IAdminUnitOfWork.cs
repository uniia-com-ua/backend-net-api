namespace UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

using Microsoft.AspNetCore.Identity;
using UniiaAdmin.Data.Dtos;

public interface IAdminUnitOfWork
{
	public IQueryable<IdentityRoleClaim<string>> RoleClaims();

	public Task<bool> CanConnectAsync();

	public Task<List<AdminUserDto>> GetListAsync();

	public Task<AdminUserDto?> GetAsync(string id);
}
