namespace UniiaAdmin.WebApi.Interfaces;

using UniiaAdmin.Data.Models;

public interface ILogPaginationService
{
	public Task<List<LogActionModel>> GetPagedListAsync(int skip, int take);

	public Task<List<LogActionModel>> GetPagedListAsync(string userId, int skip, int take);

	public Task<List<LogActionModel>> GetPagedListAsync(int modelId, string modelName, int skip, int take);
}
