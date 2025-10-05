namespace UniiaAdmin.WebApi.Services;

using Microsoft.AspNetCore.Identity;
using System.Data;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class RolePaginationService : IRolePaginationService
{
	private readonly IAdminUnitOfWork _adminUnitOfWork;
	private readonly IPaginationService _paginationService;
	private readonly IRoleRepository _roleRepository;

	public RolePaginationService
		(IAdminUnitOfWork adminUnitOfWork,
		IPaginationService paginationService,
		IRoleRepository roleRepository)
	{
		_adminUnitOfWork = adminUnitOfWork;
		_paginationService = paginationService;
		_roleRepository = roleRepository;
	}

	public async Task<PageData<string>?> GetPagedClaimsAsync(int skip, int take, string? sort = null) 
		=> await _paginationService.GetPagedListAsync(_adminUnitOfWork.RoleClaims()
			.Select(rc => rc.ClaimValue!)
			.Distinct()
			.OrderBy(o => o), skip, take, sort);

	public async Task<PageData<string>?> GetPagedClaimsAsync(string id, int skip, int take, string? sort = null) 
		=> await _paginationService.GetPagedListAsync(_adminUnitOfWork.RoleClaims()
			.Where(r => r.RoleId == id)
			.Select(rc => rc.ClaimValue!)
			.Distinct()
			.OrderBy(o => o), skip, take, sort);

	public async Task<PageData<IdentityRole>?> GetPagedRolesAsync(int skip, int take, string? sort = null) 
		=> await _paginationService.GetPagedListAsync(_roleRepository.Roles(), skip, take, sort);
}
