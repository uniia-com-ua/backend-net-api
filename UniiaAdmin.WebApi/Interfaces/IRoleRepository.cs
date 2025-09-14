namespace UniiaAdmin.WebApi.Interfaces;

using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public interface IRoleRepository
{
	public Task<IdentityRole?> FindByNameAsync(string roleName);

	public Task<IList<Claim>> GetClaimsAsync(IdentityRole role);

	public Task<IdentityResult> AddClaimAsync(IdentityRole role, Claim claim);

	public Task<IdentityResult> RemoveClaimAsync(IdentityRole role, Claim claim);

	public Task<bool> RoleExistsAsync(string roleName);

	public Task<IdentityResult> CreateAsync(IdentityRole role);

	public Task<IdentityResult> DeleteAsync(IdentityRole role);

	public IQueryable<IdentityRole> Roles();
}
