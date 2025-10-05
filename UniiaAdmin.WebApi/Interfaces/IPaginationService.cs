namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Dtos;

public interface IPaginationService
{
	public Task<PageData<T>> GetPagedListAsync<T>(
		IQueryable<T>? query,
		int skip,
		int take,
		string? sortQuery = null);
}
