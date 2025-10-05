using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq.Expressions;
using System.Reflection;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;

namespace UniiaAdmin.WebApi.Services;

public class PaginationService : IPaginationService
{
	private readonly int _maxPageSize;

	public PaginationService(IConfiguration configuration)
	{
		_maxPageSize = configuration.GetValue<int>("PageSettings:MaxPageSize");
	}

	public async Task<PageData<T>> GetPagedListAsync<T>(
		IQueryable<T>? query,
		int skip,
		int take,
		string? sortQuery = null)
	{
		if (take <= 0 || query == null)
		{
			return new PageData<T>();
		}

		if (take > _maxPageSize)
		{
			take = _maxPageSize;
		}

		if (!string.IsNullOrEmpty(sortQuery))
		{
			bool first = true;

			var orders = ParseSort(sortQuery);

			foreach (var kvp in orders)
			{
				var propertyName = kvp.Key;
				var direction = kvp.Value.ToLower();

				var propertyInfo = typeof(T).GetProperty(
					propertyName,
					BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
				);

				if (propertyInfo == null)
					continue;

				var parameter = Expression.Parameter(typeof(T), "x");
				var property = Expression.PropertyOrField(parameter, propertyName);
				var lambda = Expression.Lambda(property, parameter);

				string methodName;

				if (first)
				{
					methodName = direction == "desc" ? "OrderByDescending" : "OrderBy";
					first = false;
				}
				else
				{
					methodName = direction == "desc" ? "ThenByDescending" : "ThenBy";
				}

				var method = typeof(Queryable).GetMethods()
					.Where(m => m.Name == methodName && m.GetParameters().Length == 2)
					.Single()
					.MakeGenericMethod(typeof(T), property.Type);

				query = (IQueryable<T>)method.Invoke(null, [query, lambda])!;
			}
		}

		if (query.Provider is IAsyncQueryProvider)
		{
			return new PageData<T>
			{
				TotalCount = await query.CountAsync(),
				Items = await query.Skip(skip).Take(take).ToListAsync()
			};
		}

		return new PageData<T>
		{
			TotalCount = query.Count(),
			Items = query.Skip(skip).Take(take).ToList()
		};
	}

	private Dictionary<string, string> ParseSort(string sortParam)
	{
		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		if (string.IsNullOrWhiteSpace(sortParam))
			return result;

		var fields = sortParam.Split(',', StringSplitOptions.RemoveEmptyEntries);

		foreach (var field in fields)
		{
			if (string.IsNullOrWhiteSpace(field)) continue;

			string direction = field.StartsWith("-") ? "desc" : "asc";
			string fieldName = field.TrimStart('-');

			if (!string.IsNullOrWhiteSpace(fieldName))
			{
				result[fieldName] = direction;
			}
		}

		return result;
	}
}
