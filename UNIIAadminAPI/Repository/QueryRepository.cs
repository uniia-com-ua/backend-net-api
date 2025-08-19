namespace UniiaAdmin.WebApi.Repository;

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class QueryRepository : IQueryRepository
{
	private readonly IApplicationUnitOfWork _applicationUnitOfWork;
	private readonly IPaginationService _paginationService;

	public QueryRepository(
		IApplicationUnitOfWork applicationUnitOfWork,
		IPaginationService paginationService)
	{
		_applicationUnitOfWork = applicationUnitOfWork;
		_paginationService = paginationService;
	}

	public async Task<TEntity?> GetByIdWithIncludesAsync<TEntity>(
		Expression<Func<TEntity, bool>> predicate,
		params Expression<Func<TEntity, object>>[] includes) 
		where TEntity : class
	{
		IQueryable<TEntity> query = _applicationUnitOfWork.Query<TEntity>();

		foreach (var include in includes)
		{
			query = query.Include(include);
		}

		return await query.FirstOrDefaultAsync(predicate);
	}

	public async Task<List<T>> GetPagedAsync<T>(int skip, int take) where T : class, IEntity
	{
		return await _paginationService.GetPagedListAsync(_applicationUnitOfWork.Query<T>(), skip, take);
	}

	public async Task<List<T>?> GetByIdsAsync<T>(IEnumerable<int>? ids) where T : class, IEntity
	{
		if (ids != null)
		{
			return await _applicationUnitOfWork.Query<T>().Where(obj => ids.Contains(obj.Id)).ToListAsync();
		}
		else
		{
			return null;
		}
	}
}
