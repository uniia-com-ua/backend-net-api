using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.WebApi.Attributes;

namespace UniiaAdmin.WebApi.Controllers
{
	[Route("api/v1/log-actions")]
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
        public async Task<IActionResult> GetPagedLogs([FromQuery] int skip = 0, int take = 10)
        {
			var logActionModels = await _paginationService.GetPagedListAsync(_mongoDbContext.LogActionModels, skip, take);
                                                  
            return Ok(logActionModels);
        }

		[HttpGet("model/{id:int}")]
		[Permission(PermissionResource.Logs, CrudActions.View)]
        public async Task<IActionResult> GetLogByModelId(int id, [FromQuery] string modelName, int skip = 0, int take = 10)
        {
            var logActionModels = await _paginationService.GetPagedListAsync(
                                                        _mongoDbContext.LogActionModels
                                                        .Where(lam => lam.ModelId == id && lam.ModelName == modelName), 
                                                        skip, 
                                                        take);

            return Ok(logActionModels);
        }

		[HttpGet("user/{id}")]
		[Permission(PermissionResource.Logs, CrudActions.View)]
        public async Task<IActionResult> GetByUserId(string id, [FromQuery] int skip = 0, int take = 10)
        {
            var logActionModels = await _paginationService.GetPagedListAsync(
                                                        _mongoDbContext.LogActionModels
                                                        .Where(lam => lam.UserId == id),
                                                        skip,
                                                        take);

            return Ok(logActionModels);
        }
    }
}
