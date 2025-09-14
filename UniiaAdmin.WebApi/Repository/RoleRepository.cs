namespace UniiaAdmin.WebApi.Repository;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Resources;

public class RoleRepository : IRoleRepository
{
	private readonly RoleManager<IdentityRole> _roleManager;

	public RoleRepository(RoleManager<IdentityRole> roleManager)
	{
		_roleManager = roleManager;
	}

	public Task<IdentityResult> AddClaimAsync(IdentityRole role, Claim claim) => _roleManager.AddClaimAsync(role, claim);

	public Task<IdentityResult> CreateAsync(IdentityRole role) => _roleManager.CreateAsync(role);

	public Task<IdentityResult> DeleteAsync(IdentityRole role) => _roleManager.DeleteAsync(role);

	public Task<IdentityRole?> FindByNameAsync(string roleName) => _roleManager.FindByNameAsync(roleName);

	public Task<IList<Claim>> GetClaimsAsync(IdentityRole role) => _roleManager.GetClaimsAsync(role);

	public Task<IdentityResult> RemoveClaimAsync(IdentityRole role, Claim claim) => _roleManager.RemoveClaimAsync(role, claim);

	public Task<bool> RoleExistsAsync(string roleName) => _roleManager.RoleExistsAsync(roleName);

	public IQueryable<IdentityRole> Roles() => _roleManager.Roles;
}
