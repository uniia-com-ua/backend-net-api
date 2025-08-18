namespace UniiaAdmin.WebApi.Services;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;

public class EntityQueryService : IEntityQueryService
{

	private readonly ApplicationContext _dbContext;

	public EntityQueryService(ApplicationContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<List<T>?> GetByIdsAsync<T>(IEnumerable<int>? ids) where T : class, IEntity
	{
		if (ids != null)
		{
			return await _dbContext.Set<T>().Where(obj => ids.Contains(obj.Id)).ToListAsync();
		}
		else
		{
			return null;
		}	
	}
}
