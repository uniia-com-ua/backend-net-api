namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface ISmallPhotoRepository
{
	public Task<Result<K>> GetSmallPhotoAsync<T, K>(int id)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity;

	public Task<Result<K>> CreateAsync<T, K>(T model, IFormFile? photo, IFormFile? smallPhoto)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity, new();

	public Task<Result<K>> UpdateAsync<T, K>(T model, T existedModel, IFormFile? photo, IFormFile? smallPhoto)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity, new();
	public Task DeleteAsync<T, K>(T model)
			where T : class, ISmallPhotoEntity
			where K : class, IMongoFileEntity;
}
