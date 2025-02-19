using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Services;

namespace UniiaAdmin.WebApi.Controllers
{
    [Route("user-actions")]
    [ApiController]
    public class LogActionController : ControllerBase
    {
        private readonly MongoDbContext _mongoDbContext;
        private readonly IMapper _mapper;
        public LogActionController(MongoDbContext mongoDbContext,
            IMapper mapper) 
        {
            _mongoDbContext = mongoDbContext;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPagedLogs(int skip, int take)
        {
            var logActionModels = await PaginationHelper.GetPagedListAsync(_mongoDbContext.LogActionModels, skip, take);

            var resultList = logActionModels.Select(a => _mapper.Map<LogActionModelDto>(a));
                                                  
            return Ok(resultList);
        }

        [HttpGet]
        [Route("get-by-model")]
        public async Task<IActionResult> GetLogByModelId(int modelId, string modelName, int skip, int take)
        {
            var logActionModels = await PaginationHelper.GetPagedListAsync(
                                                        _mongoDbContext.LogActionModels
                                                        .Where(lam => lam.ModelId == modelId && lam.ModelName == modelName), 
                                                        skip, 
                                                        take);

            var resultList = logActionModels.Select(a => _mapper.Map<LogActionModelDto>(a));

            return Ok(resultList);
        }

        [HttpGet]
        [Route("get-by-userid")]
        public async Task<IActionResult> GetByUserId(string userId, int skip, int take)
        {
            var logActionModels = await PaginationHelper.GetPagedListAsync(
                                                        _mongoDbContext.LogActionModels
                                                        .Where(lam => lam.UserId == userId),
                                                        skip,
                                                        take);

            var resultList = logActionModels.Select(a => _mapper.Map<LogActionModelDto>(a));

            return Ok(resultList);
        }
    }
}
