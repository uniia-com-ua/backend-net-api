namespace UniiaAdmin.WebApi.Interfaces;

public interface IPaginationService
{
	public Task<List<T>> GetPagedListAsync<T>(IQueryable<T>? query, int skip, int take);
}
