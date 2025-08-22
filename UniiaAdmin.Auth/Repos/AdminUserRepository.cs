namespace UniiaAdmin.Auth.Repos;

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Models;

public class AdminUserRepository : IAdminUserRepository
{
	private readonly UserManager<AdminUser> _userManager;

	public AdminUserRepository(UserManager<AdminUser> userManager)
	{
		_userManager = userManager;
	}

	public async Task<AdminUser?> GetUserAsync(ClaimsPrincipal claims) => await _userManager.GetUserAsync(claims);

	public async Task<IdentityResult> UpdateAsync(AdminUser user) => await _userManager.UpdateAsync(user);

	public async Task<IdentityResult> SetAuthenticationTokenAsync(AdminUser user, string loginProvider, string tokenName, string? tokenValue) 
		=> await _userManager.SetAuthenticationTokenAsync(user, loginProvider, tokenName, tokenValue);

	public async Task<AdminUser?> FindByIdAsync(string id) => await _userManager.FindByIdAsync(id);

	public async Task<string?> GetAuthenticationTokenAsync(AdminUser user, string loginProvider, string tokenName)
		=> await _userManager.GetAuthenticationTokenAsync(user, loginProvider, tokenName);
}
