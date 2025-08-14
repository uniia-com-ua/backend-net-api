using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;

namespace UniiaAdmin.WebApi.Controllers
{
	[Route("api/v1/user-actions")]
    [ApiController]
    public class LogActionController : ControllerBase
    {
        private readonly MongoDbContext _mongoDbContext;
        private readonly IMapper _mapper;
        private readonly IPaginationService _paginationService;
        public LogActionController(MongoDbContext mongoDbContext,
            IMapper mapper,
            IPaginationService paginationService) 
        {
            _mongoDbContext = mongoDbContext;
            _mapper = mapper;
            _paginationService = paginationService;
        }

		[HttpGet("page")]
		[Permission(PermissionResource.Logs, CrudActions.View)]
        public async Task<IActionResult> GetPagedLogs(int skip = 0, int take = 10)
        {
			var logActionModels = await _paginationService.GetPagedListAsync(_mongoDbContext.LogActionModels, skip, take);

			var resultList = logActionModels.Select(a => _mapper.Map<LogActionModelDto>(a));
                                                  
            return Ok(resultList);
        }

		[HttpGet("model/{id:int}")]
		[Permission(PermissionResource.Logs, CrudActions.View)]
        public async Task<IActionResult> GetLogByModelId(int id, string modelName, int skip, int take)
        {
            var logActionModels = await _paginationService.GetPagedListAsync(
                                                        _mongoDbContext.LogActionModels
                                                        .Where(lam => lam.ModelId == id && lam.ModelName == modelName), 
                                                        skip, 
                                                        take);

            var resultList = logActionModels.Select(a => _mapper.Map<LogActionModelDto>(a));

            return Ok(resultList);
        }

		[HttpGet("{id}")]
		[Permission(PermissionResource.Logs, CrudActions.View)]
        public async Task<IActionResult> GetByUserId(string id, int skip = 0, int take = 10)
        {
            var logActionModels = await _paginationService.GetPagedListAsync(
                                                        _mongoDbContext.LogActionModels
                                                        .Where(lam => lam.UserId == id),
                                                        skip,
                                                        take);

            var resultList = logActionModels.Select(a => _mapper.Map<LogActionModelDto>(a));

            return Ok(resultList);
        }
    }
}
