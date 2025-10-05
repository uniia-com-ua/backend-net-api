namespace UniiaAdmin.WebApi.Interfaces;

using Microsoft.AspNetCore.Identity;
using UniiaAdmin.Data.Dtos;

public interface IRolePaginationService
{
	public Task<PageData<IdentityRole>?> GetPagedRolesAsync(int skip, int take, string? sort = null);

	public Task<PageData<string>?> GetPagedClaimsAsync(int skip, int take, string? sort = null);

	public Task<PageData<string>?> GetPagedClaimsAsync(string id, int skip, int take, string? sort = null);
}
