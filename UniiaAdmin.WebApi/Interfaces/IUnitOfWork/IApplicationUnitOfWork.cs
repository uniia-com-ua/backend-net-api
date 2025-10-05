namespace UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

using System.Linq.Expressions;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IApplicationUnitOfWork
{
	public Task<T?> FindAsync<T>(int id) where T : class, IEntity;

	public Task<T?> FindAsync<T>(string id) where T : class, IStringEntity;

	public IQueryable<T> Query<T>(Expression<Func<T, bool>> predicate) where T : class;

	public Task<string?> FindPhotoIdAsync<T>(int id) where T : class, IPhotoEntity;

	public Task<string?> FindSmallPhotoIdAsync<T>(int id) where T : class, ISmallPhotoEntity;

	public Task<string?> FindFileIdAsync<T>(int id) where T : class, IFileEntity;

	public Task AddAsync<T>(T model) where T : class;

	public Task<bool> AnyAsync<T>(int id) where T : class, IEntity;

	public Task<bool> AnyAsync<T>(string id) where T : class, IStringEntity;

	public Task<bool> AnyEmailAsync<T>(string? email) where T : class, IEmailEntity;

	public void Remove<T>(T model) where T : class;

	public Task CreateDatabaseAsync();

	public Task<PageData<T>> GetPagedAsync<T>(int skip, int take, string? sortQuery = null) where T : class;

	public Task<T?> GetByIdWithIncludesAsync<T>(
		Expression<Func<T, bool>> predicate,
		params Expression<Func<T, object>>[] includes)
		where T : class;

	public Task<PageData<TEntity>> GetPagedWithIncludesAsync<TEntity>(
		int skip,
		int take,
		string? sort = null,
		params Expression<Func<TEntity, object>>[] includes)
		where TEntity : class;

	public Task<List<T>?> GetByIdsAsync<T>(IEnumerable<int>? ids) where T : class, IEntity;

	public Task<bool> CanConnectAsync();

	public Task SaveChangesAsync();
}
