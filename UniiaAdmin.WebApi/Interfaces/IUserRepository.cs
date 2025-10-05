namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Dtos.UserDtos;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.WebApi.Services;

public interface IUserRepository
{
	public Task<string> CreateAsync(UserDto userCreationDto);

	public Task<UserDto> FindAsync(string id);

	public Task<PageData<UserDto>?> GetPagedAsync(AccountStatus? accountStatus, int skip, int take, string? sort = null);

	public Task<bool> IsEmailExistAsync(string? email);

	public Task UpdateAsync(string id, UserDto userCreationDto);
}
