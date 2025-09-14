namespace UniiaAdmin.WebApi.Services;

using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

public class LogPaginationService : ILogPaginationService
{
	private readonly IMongoUnitOfWork _mongoUnitOfWork;
	private readonly IPaginationService _paginationService;
	public LogPaginationService(
		IMongoUnitOfWork mongoUnitOfWork,
		IPaginationService paginationService)
	{
		_mongoUnitOfWork = mongoUnitOfWork;
		_paginationService = paginationService;
	}
	public async Task<List<LogActionModel>> GetPagedListAsync(string userId, int skip, int take) 
		=>  await _paginationService.GetPagedListAsync(_mongoUnitOfWork.Query<LogActionModel>(lam => lam.UserId == userId), 
                                                        skip, 
                                                        take);

	public async Task<List<LogActionModel>> GetPagedListAsync(int modelId, string modelName, int skip, int take)
		=> await _paginationService.GetPagedListAsync(_mongoUnitOfWork.Query<LogActionModel>(lam => lam.ModelId == modelId && lam.ModelName == modelName),
														skip,
														take);

	public async Task<List<LogActionModel>> GetPagedListAsync(int skip, int take)
		=> await _paginationService.GetPagedListAsync(_mongoUnitOfWork.Query<LogActionModel>(), skip, take);
}
