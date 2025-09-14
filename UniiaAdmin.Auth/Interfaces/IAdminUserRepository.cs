namespace UniiaAdmin.Auth.Interfaces;

using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UniiaAdmin.Data.Models;

public interface IAdminUserRepository
{
	public Task<AdminUser?> GetUserAsync(ClaimsPrincipal claims);

	public Task<IdentityResult> UpdateAsync(AdminUser user);

	public Task<IdentityResult> SetAuthenticationTokenAsync(AdminUser user, string loginProvider, string tokenName, string? tokenValue);

	public Task<AdminUser?> FindByIdAsync(string id);

	public Task<string?> GetAuthenticationTokenAsync(AdminUser user, string loginProvider, string tokenName);
}
