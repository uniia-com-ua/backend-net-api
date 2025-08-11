using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.WebApi.Controllers
{
	[Authorize]
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

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPagedLogs(int skip = 0, int take = 10)
        {
            var logActionModels = await _paginationService.GetPagedListAsync(_mongoDbContext.LogActionModels, skip, take);

            var resultList = logActionModels.Select(a => _mapper.Map<LogActionModelDto>(a));
                                                  
            return Ok(resultList);
        }

        [HttpGet]
        [Route("get-by-model")]
        public async Task<IActionResult> GetLogByModelId(int modelId, string modelName, int skip, int take)
        {
            var logActionModels = await _paginationService.GetPagedListAsync(
                                                        _mongoDbContext.LogActionModels
                                                        .Where(lam => lam.ModelId == modelId && lam.ModelName == modelName), 
                                                        skip, 
                                                        take);

            var resultList = logActionModels.Select(a => _mapper.Map<LogActionModelDto>(a));

            return Ok(resultList);
        }

        [HttpGet]
        [Route("get-by-userid")]
        public async Task<IActionResult> GetByUserId(string userId, int skip = 0, int take = 10)
        {
            var logActionModels = await _paginationService.GetPagedListAsync(
                                                        _mongoDbContext.LogActionModels
                                                        .Where(lam => lam.UserId == userId),
                                                        skip,
                                                        take);

            var resultList = logActionModels.Select(a => _mapper.Map<LogActionModelDto>(a));

            return Ok(resultList);
        }
    }
}
