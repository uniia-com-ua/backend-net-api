namespace UniiaAdmin.WebApi.Repository;

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

	public async Task<T?> GetByIdAsync<T>(int id) where T : class
	{
		return await _applicationContext.Set<T>().FindAsync(id);
	}

	public async Task<List<T>> GetPagedAsync<T>(int skip, int take) where T : class, IEntity
	{
		return await _paginationService.GetPagedListAsync(_applicationContext.Set<T>(), skip, take);
	}
}
