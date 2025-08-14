using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Interfaces;

namespace UniiaAdmin.WebApi.Services;

public class PaginationService : IPaginationService
{
	private readonly int _maxPageSize;

	public PaginationService(IConfiguration configuration)
	{
		_maxPageSize = configuration.GetValue<int>("PageSettings:MaxPageSize");
	}

	public async Task<List<T>> GetPagedListAsync<T>(IQueryable<T> query, int skip, int take)
			where T : class
	{
		if (take <= 0)
		{
			return [];
		}

		if (take > _maxPageSize)
		{
			take = _maxPageSize;
		}

		var result = await query.Skip(skip).Take(take).ToListAsync();

		return result;
	}
}
