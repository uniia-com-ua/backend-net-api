namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IQueryRepository
{
	public Task<T?> GetByIdAsync<T>(int id) where T : class;

	public Task<List<T>> GetPagedAsync<T>(int skip, int take) where T : class, IEntity;
}
