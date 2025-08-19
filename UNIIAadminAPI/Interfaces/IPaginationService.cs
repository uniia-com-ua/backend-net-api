namespace UniiaAdmin.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IPaginationService
{
	public Task<List<T>> GetPagedListAsync<T>(IQueryable<T>? query, int skip, int take);
}
