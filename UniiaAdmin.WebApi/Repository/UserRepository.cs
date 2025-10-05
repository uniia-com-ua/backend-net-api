namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Dtos.UserDtos;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Services;

public class UserRepository : IUserRepository
{
	private readonly IApplicationUnitOfWork _applicationUnitOfWork;
	private readonly IPaginationService _paginationService;
	private readonly IMapper _mapper;

	public UserRepository(
		IApplicationUnitOfWork applicationUnitOfWork,
		IPaginationService paginationService,
		IMapper mapper)
	{
		_applicationUnitOfWork = applicationUnitOfWork;
		_mapper = mapper;
		_paginationService = paginationService;
	}

	public async Task<string> CreateAsync(UserDto userCreationDto)
	{
		userCreationDto.Id = Guid.NewGuid().ToString();
		userCreationDto.RegistrationDate = DateTime.UtcNow;
		userCreationDto.AccountStatus = AccountStatus.PENDING;

		var user = _mapper.Map<User>(userCreationDto);

		await _applicationUnitOfWork.AddAsync(user);

		await _applicationUnitOfWork.SaveChangesAsync();

		return user.Id;
	}

	public async Task<UserDto> FindAsync(string id)
	{
		var user = await _applicationUnitOfWork.FindAsync<User>(id);

		return _mapper.Map<UserDto>(user);
	}

	public async Task<PageData<UserDto>?> GetPagedAsync(AccountStatus? accountStatus, int skip, int take, string? sort = null)
	{
		var users = await _paginationService.GetPagedListAsync(
			_applicationUnitOfWork.Query<User>(x => accountStatus == null || x.AccountStatus == accountStatus), 
			skip, 
			take, 
			sort);

		return new PageData<UserDto>
		{
			Items = users.Items.Select(u => _mapper.Map<UserDto>(u)).ToList(),
			TotalCount = users.TotalCount
		};
	}

	public async Task<bool> IsEmailExistAsync(string email)
	{
		return await _applicationUnitOfWork.AnyEmailAsync<User>(email);
	}

	public async Task<bool> IsExistAsync(string id)
	{
		return await _applicationUnitOfWork.AnyAsync<User>(id);
	}

	public async Task UpdateAsync(string id, UserDto userCreationDto)
	{
		var user = await _applicationUnitOfWork.FindAsync<User>(id);

		_mapper.Map(userCreationDto, user);

		await _applicationUnitOfWork.SaveChangesAsync();
	}
}
