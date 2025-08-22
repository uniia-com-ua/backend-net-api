namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IPhotoProvider
{
	public Task<Result<K>> GetPhotoAsync<T, K>(int id)
		where T : class, IPhotoEntity
		where K : class, IMongoFileEntity;
}
