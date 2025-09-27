namespace UniiaAdmin.WebApi.Repository;

using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using UniiaAdmin.Data.Dtos.UserDtos;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class UserRepository : IUserRepository
{
	private readonly IApplicationUnitOfWork _applicationUnitOfWork;
	private readonly IMapper _mapper;

	public UserRepository(
		IApplicationUnitOfWork applicationUnitOfWork,
		IMapper mapper)
	{
		_applicationUnitOfWork = applicationUnitOfWork;
		_mapper = mapper;
	}

	public async Task<string> CreateAsync(UserDto userCreationDto)
	{
		userCreationDto.Id = Guid.NewGuid().ToString();
		userCreationDto.RegistrationDate = DateTime.UtcNow;
		userCreationDto.AccountStatus = AccountStatus.PENDING;

		var user = _mapper.Map<User>(userCreationDto);

		_applicationUnitOfWork.Create(user);

		await _applicationUnitOfWork.SaveChangesAsync();

		return user.Id;
	}

	public async Task<UserDto> FindAsync(string id)
	{
		var user = await _applicationUnitOfWork.FindAsync<User>(id);

		return _mapper.Map<UserDto>(user);
	}

	public async Task<bool> IsEmailExistAsync(string email)
	{
		return await _applicationUnitOfWork.GetQueryable<User>().AnyAsync(u => u.Email == email);
	}

	public async Task UpdateAsync(string id, UserDto userCreationDto)
	{
		var user = await _applicationUnitOfWork.FindAsync<User>(id);

		_mapper.Map(userCreationDto, user);

		await _applicationUnitOfWork.SaveChangesAsync();
	}
}
