namespace UniiaAdmin.WebApi.Repository;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;

public class QueryRepository : IQueryRepository
{
	private readonly ApplicationContext _applicationContext;
	private readonly IPaginationService _paginationService;

	public QueryRepository(
		ApplicationContext applicationContext,
		IPaginationService paginationService)
	{
		_applicationContext = applicationContext;
		_paginationService = paginationService;
	}

	public async Task<bool> AnyAsync<T>(int id) where T : class, IEntity
	{
		return await _applicationContext.Set<T>().AnyAsync(o => o.Id == id);
	}

	public async Task<T?> GetByIdAsync<T>(int id) where T : class
	{
		return await _applicationContext.Set<T>().FindAsync(id);
	}

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

	public async Task<List<T>> GetPagedAsync<T>(int skip, int take) where T : class, IEntity
	{
		return await _paginationService.GetPagedListAsync(_applicationContext.Set<T>(), skip, take);
	}
}
