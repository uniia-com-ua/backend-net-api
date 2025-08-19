namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IPhotoRepository
{
	public Task<Result<K>> CreateAsync<T, K>(T model, IFormFile? photo)
		where T : class, IPhotoEntity
		where K : class, IMongoFileEntity, new();

	public Task<Result<K>> UpdateAsync<T, K>(T model, T existedModel, IFormFile? photo)
		where T : class, IPhotoEntity
		where K : class, IMongoFileEntity, new();

	public Task DeleteAsync<T, K>(T model)
		where T : class, IPhotoEntity
		where K : class, IMongoFileEntity;
}
