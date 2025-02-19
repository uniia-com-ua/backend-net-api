using Microsoft.EntityFrameworkCore;
using UniiaAdmin.WebApi.Constants;

namespace UniiaAdmin.WebApi.Services
{
    public static class PaginationHelper
    {
        public static async Task<List<T>> GetPagedListAsync<T>(IQueryable<T> query, int skip, int take) 
            where T : class
        {
            if (take <= 0)
            {
                return [];
            }

            if (take > AppConstants.MaxPageSize)
            {
                take = AppConstants.MaxPageSize;
            }

            var result = await query.Skip(skip).Take(take).ToListAsync();

            return result;
        }
    }
}
