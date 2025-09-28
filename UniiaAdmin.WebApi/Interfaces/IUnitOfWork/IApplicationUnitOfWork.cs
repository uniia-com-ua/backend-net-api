namespace UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

using System.Linq.Expressions;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IApplicationUnitOfWork
{
	public IQueryable<T> GetQueryable<T>() where T : class;

	public Task<T?> FindAsync<T>(object id) where T : class;

	public Task<string?> FindPhotoIdAsync<T>(int id) where T : class, IPhotoEntity;

	public Task<string?> FindSmallPhotoIdAsync<T>(int id) where T : class, ISmallPhotoEntity;

	public Task<string?> FindFileIdAsync<T>(int id) where T : class, IFileEntity;

	public Task AddAsync<T>(T model) where T : class;

	public Task<bool> AnyAsync<T>(int id) where T : class, IEntity;

	public void Attach<T>(T model) where T : class;

	public void Remove<T>(T model) where T : class;

	public void Update<T>(T model) where T : class;

	public void Create<T>(T model) where T : class;

	public Task CreateDatabaseAsync();

	public Task<List<T>> GetPagedAsync<T>(int skip, int take) where T : class;

	public Task<TEntity?> GetByIdWithIncludesAsync<TEntity>(
		Expression<Func<TEntity, bool>> predicate,
		params Expression<Func<TEntity, object>>[] includes)
		where TEntity : class;

	public Task<List<TEntity>> GetPagedWithIncludesAsync<TEntity>(
		int skip,
		int take,
		params Expression<Func<TEntity, object>>[] includes)
		where TEntity : class;

	public Task<List<T>?> GetByIdsAsync<T>(IEnumerable<int>? ids) where T : class, IEntity;

	public Task<bool> CanConnectAsync();

	public Task SaveChangesAsync();
}
