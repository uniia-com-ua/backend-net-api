using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces;

namespace UniiaAdmin.WebApi.Controllers
{
	[Route("api/v1/log-actions")]
    [ApiController]
    public class LogActionController : ControllerBase
    {
        private readonly ILogPaginationService _paginationService;
        public LogActionController(ILogPaginationService paginationService) 
        {
            _paginationService = paginationService;
        }

		[HttpGet("page")]
		[Permission(PermissionResource.Logs, CrudActions.View)]
        public async Task<IActionResult> GetPagedLogs([FromQuery] string? sort = null, int skip = 0, int take = 10)
        {
			var logActionModels = await _paginationService.GetPagedListAsync(skip, take, sort);
                                                  
            return Ok(logActionModels);
        }

		[HttpGet("model/{id:int}")]
		[Permission(PermissionResource.Logs, CrudActions.View)]
        public async Task<IActionResult> GetLogByModelId(int id, [FromQuery] string modelName, string? sort = null, int skip = 0, int take = 10)
        {
            var logActionModels = await _paginationService.GetPagedListAsync(id, modelName, skip, take, sort);

            return Ok(logActionModels);
        }

		[HttpGet("user/{id}")]
		[Permission(PermissionResource.Logs, CrudActions.View)]
        public async Task<IActionResult> GetByUserId(string id, [FromQuery] string? sort = null, int skip = 0, int take = 10)
        {
            var logActionModels = await _paginationService.GetPagedListAsync(id, skip, take, sort);

			return Ok(logActionModels);
        }
    }
}
