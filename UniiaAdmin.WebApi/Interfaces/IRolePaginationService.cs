namespace UniiaAdmin.WebApi.Interfaces;

using Microsoft.AspNetCore.Identity;

public interface IRolePaginationService
{
	public Task<List<IdentityRole>?> GetPagedRolesAsync(int skip, int take);

	public Task<List<string>?> GetPagedClaimsAsync(int skip, int take);

	public Task<List<string>?> GetPagedClaimsAsync(string id, int skip, int take);
}
