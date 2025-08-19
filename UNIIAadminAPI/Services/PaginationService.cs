using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using UniiaAdmin.Data.Interfaces;

namespace UniiaAdmin.WebApi.Services;

public class PaginationService : IPaginationService
{
	private readonly int _maxPageSize;

	public PaginationService(IConfiguration configuration)
	{
		_maxPageSize = configuration.GetValue<int>("PageSettings:MaxPageSize");
	}

	public async Task<List<T>> GetPagedListAsync<T>(IQueryable<T>? query, int skip, int take)
	{
		if (take <= 0 || query == null)
		{
			return [];
		}

		if (take > _maxPageSize)
		{
			take = _maxPageSize;
		}

		if (query.Provider is IAsyncQueryProvider)
			return await query.Skip(skip).Take(take).ToListAsync();

		return query.Skip(skip).Take(take).ToList();
	}
}
