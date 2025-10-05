namespace UniiaAdmin.WebApi.Repository.UnitOfWork;

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Dtos.UserDtos;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class AdminUnitOfWork : IAdminUnitOfWork
{
	private readonly AdminContext _adminContext;
	private readonly UserManager<AdminUser> _userManager;
	private readonly IMapper _mapper;

	public AdminUnitOfWork(
		AdminContext adminContext,
		UserManager<AdminUser> userManager,
		IMapper mapper)
	{
		_adminContext = adminContext;
		_userManager = userManager;
		_mapper = mapper;
	}

	public IQueryable<IdentityRoleClaim<string>> RoleClaims()
	=> _adminContext.RoleClaims;

	public async Task<bool> CanConnectAsync()
		=> await _adminContext.Database.CanConnectAsync();

	public async Task<PageData<AdminUserDto>> GetListAsync()
	{
		var result = new PageData<AdminUserDto>();

		var users = await _userManager.Users.ToListAsync();

		foreach(var user in users)
		{
			var roles = await _userManager.GetRolesAsync(user);

			var mappedUser = _mapper.Map<AdminUserDto>(user);

			mappedUser.Roles = roles.ToList();

			result.Items.Add(mappedUser);
		}

		result.TotalCount = result.Items.Count;

		return result;
	}
		

	public async Task<AdminUserDto?> GetAsync(string id)
	{
		var user = await _userManager.FindByIdAsync(id);

		if (user == null) return null;

		var roles = await _userManager.GetRolesAsync(user);

		var mappedUser = _mapper.Map<AdminUserDto>(user);

		mappedUser.Roles = roles.ToList();

		return mappedUser;
	}
}
