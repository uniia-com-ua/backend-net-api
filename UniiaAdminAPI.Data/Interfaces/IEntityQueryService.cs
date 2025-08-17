namespace UniiaAdmin.Data.Interfaces;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

public interface IEntityQueryService
{
	public Task<List<T>> GetByIdsAsync<T>(DbSet<T> dbSet, IEnumerable<int>? ids) where T : class, IEntity;
}
