namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Models;

public interface ILogPaginationService
{
	public Task<PageData<LogActionModel>> GetPagedListAsync(int skip, int take, string? sortQuery = null);

	public Task<PageData<LogActionModel>> GetPagedListAsync(string userId, int skip, int take, string? sortQuery = null);

	public Task<PageData<LogActionModel>> GetPagedListAsync(int modelId, string modelName, int skip, int take, string? sortQuery = null);
}
