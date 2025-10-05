namespace UniiaAdmin.WebApi.Services;

using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
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
	public async Task<PageData<LogActionModel>> GetPagedListAsync(string userId, int skip, int take, string? sortQuery = null) 
		=>  await _paginationService.GetPagedListAsync(_mongoUnitOfWork.Query<LogActionModel>(lam => lam.UserId == userId), 
                                                        skip, 
                                                        take,
														sortQuery);

	public async Task<PageData<LogActionModel>> GetPagedListAsync(int modelId, string modelName, int skip, int take, string? sortQuery = null)
		=> await _paginationService.GetPagedListAsync(_mongoUnitOfWork.Query<LogActionModel>(lam => lam.ModelId == modelId && lam.ModelName == modelName),
														skip,
														take,
														sortQuery);

	public async Task<PageData<LogActionModel>> GetPagedListAsync(int skip, int take, string? sortQuery = null)
		=> await _paginationService.GetPagedListAsync(_mongoUnitOfWork.Query<LogActionModel>(), skip, take, sortQuery);
}
