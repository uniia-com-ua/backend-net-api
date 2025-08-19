namespace UniiaAdmin.WebApi.Interfaces;

using System.Linq.Expressions;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IQueryRepository
{
	public Task<List<T>> GetPagedAsync<T>(int skip, int take) where T : class, IEntity;

	public Task<TEntity?> GetByIdWithIncludesAsync<TEntity>(
		Expression<Func<TEntity, bool>> predicate,
		params Expression<Func<TEntity, object>>[] includes)
		where TEntity : class;

	public Task<List<T>?> GetByIdsAsync<T>(IEnumerable<int>? ids) where T : class, IEntity;
}
