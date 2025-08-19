namespace UniiaAdmin.WebApi.Repository.UnitOfWork;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class ApplicationUnitOfWork : IApplicationUnitOfWork
{
	private readonly ApplicationContext _applicationContext;
	private readonly IPaginationService _paginationService;

	public ApplicationUnitOfWork(
		ApplicationContext applicationContext, 
		IPaginationService paginationService)
	{
		_applicationContext = applicationContext;
		_paginationService = paginationService;
	}

	public async Task AddAsync<T>(T model) where T : class => await _applicationContext.Set<T>().AddAsync(model);

	public async Task<T?> FindAsync<T>(int id) where T : class, IEntity => await _applicationContext.Set<T>().FindAsync(id);

	public async Task SaveChangesAsync() => await _applicationContext.SaveChangesAsync();

	public async Task<bool> AnyAsync<T>(int id) where T : class, IEntity => await _applicationContext.Set<T>().AnyAsync(e => e.Id == id);

	public void Attach<T>(T model) where T : class => _applicationContext.Set<T>().Attach(model);

	public void Remove<T>(T model) where T : class => _applicationContext.Set<T>().Remove(model);

	public void Update<T>(T model) where T : class => _applicationContext.Set<T>().Update(model);

	public async Task<bool> CanConnectAsync() => await _applicationContext.Database.CanConnectAsync();

	public async Task CreateAsync() => await _applicationContext.Database.EnsureCreatedAsync();

	public async Task<string?> FindPhotoIdAsync<T>(int id) where T : class, IPhotoEntity 
		=> await _applicationContext.Set<T>()
			.Where(a => a.Id == id)
			.Select(a => a.PhotoId)
			.FirstOrDefaultAsync();

	public async Task<string?> FindSmallPhotoIdAsync<T>(int id) where T : class, ISmallPhotoEntity
		=> await _applicationContext.Set<T>()
			.Where(a => a.Id == id)
			.Select(a => a.SmallPhotoId)
			.FirstOrDefaultAsync();
	public async Task<string?> FindFileIdAsync<T>(int id) where T : class, IFileEntity
				=> await _applicationContext.Set<T>()
			.Where(a => a.Id == id)
			.Select(a => a.FileId)
			.FirstOrDefaultAsync();

	public async Task<List<T>> GetPagedAsync<T>(int skip, int take) where T : class, IEntity
		=> await _paginationService.GetPagedListAsync(_applicationContext.Set<T>(), skip, take);

	public async Task<TEntity?> GetByIdWithIncludesAsync<TEntity>(
		Expression<Func<TEntity, bool>> predicate,
		params Expression<Func<TEntity, object>>[] includes)
		where TEntity : class
	{
		IQueryable<TEntity> query = _applicationContext.Set<TEntity>();

		foreach (var include in includes)
		{
			query = query.Include(include);
		}

		return await query.FirstOrDefaultAsync(predicate);
	}

	public async Task<List<T>?> GetByIdsAsync<T>(IEnumerable<int>? ids) where T : class, IEntity
	{
		if (ids == null)
			return null;

		return await _applicationContext.Set<T>()
			.Where(obj => ids.Contains(obj.Id))
			.ToListAsync();
	}

}
