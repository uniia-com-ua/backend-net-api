namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Dtos.UserDtos;

public interface IUserRepository
{
	public Task<string> CreateAsync(UserDto userCreationDto);

	public Task<UserDto> FindAsync(string id);

	public Task<bool> IsEmailExistAsync(string email);

	public Task UpdateAsync(string id, UserDto userCreationDto);
}
