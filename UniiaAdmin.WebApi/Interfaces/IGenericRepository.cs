namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IGenericRepository
{
	public Task CreateAsync<T>(T model) where T : class, IEntity;

	public Task UpdateAsync<T>(T model, T existedModel) where T : class, IEntity;

	public Task DeleteAsync<T>(T model) where T : class;
}
