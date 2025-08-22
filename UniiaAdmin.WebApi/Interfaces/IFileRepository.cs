namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IFileRepository
{
	public Task<Result<K>> GetFileAsync<T, K>(int id)
		where T : class, IFileEntity
		where K : class, IMongoFileEntity;

	public Task<Result<K>> CreateAsync<T, K>(T model, IFormFile? file) 
		where T : class, IFileEntity
		where K : class, IMongoFileEntity, new();

	public Task<Result<K>> UpdateAsync<T, K>(T model, T existedModel, IFormFile? file) 
		where T : class, IFileEntity
		where K : class, IMongoFileEntity, new();

	public Task DeleteAsync<T, K>(T model) 
		where T : class, IFileEntity
		where K : class, IMongoFileEntity;
}
