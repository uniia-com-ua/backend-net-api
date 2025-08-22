namespace UniiaAdmin.WebApi.Services;

using Microsoft.AspNetCore.Identity;
using System.Data;
using UniiaAdmin.Data.Data;
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

	public async Task<List<string>?> GetPagedClaimsAsync(int skip, int take) 
		=> await _paginationService.GetPagedListAsync(_adminUnitOfWork.RoleClaims()
			.Select(rc => rc.ClaimValue!)
			.Distinct()
			.OrderBy(o => o), skip, take);

	public async Task<List<string>?> GetPagedClaimsAsync(string id, int skip, int take) 
		=> await _paginationService.GetPagedListAsync(_adminUnitOfWork.RoleClaims()
			.Where(r => r.RoleId == id)
			.Select(rc => rc.ClaimValue!)
			.Distinct()
			.OrderBy(o => o), skip, take);

	public async Task<List<IdentityRole>?> GetPagedRolesAsync(int skip, int take) 
		=> await _paginationService.GetPagedListAsync(_roleRepository.Roles(), skip, take);
}
