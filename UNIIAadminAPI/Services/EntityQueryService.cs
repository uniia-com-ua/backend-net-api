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
	public async Task<List<T>?> GetByIdsAsync<T>(DbSet<T>? dbSet, IEnumerable<int>? ids) where T : class, IEntity
	{
		if (dbSet != null && ids != null)
		{
			return await dbSet.Where(obj => ids.Contains(obj.Id)).ToListAsync();
		}
		else
		{
			return null;
		}	
	}
}
