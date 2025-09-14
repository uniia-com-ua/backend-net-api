using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UniiaAdmin.WebApi.Controllers
{
	[ApiController]
    [Route("api/v1/keywords")]
    public class KeywordController : ControllerBase
    {
		private readonly IGenericRepository _genericRepository;
		private readonly IApplicationUnitOfWork _applicationUnitOfWork;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public KeywordController(
			IGenericRepository genericRepository,
			IStringLocalizer<ErrorMessages> localizer,
			IApplicationUnitOfWork applicationUnitOfWork)
		{
			_localizer = localizer;
			_genericRepository = genericRepository;
			_applicationUnitOfWork = applicationUnitOfWork;
		}

		[HttpGet("{id:int}")]
		[Permission(PermissionResource.Keyword, CrudActions.View)]
        public async Task<IActionResult> Get(int id)
        {
            var keyword = await _applicationUnitOfWork.FindAsync<Keyword>(id);

			if (keyword == null)
                return NotFound(_localizer["ModelNotFound", nameof(Keyword), id.ToString()].Value);

            return Ok(keyword);
        }

		[HttpGet("page")]
		[Permission(PermissionResource.Keyword, CrudActions.View)]
        public async Task<IActionResult> GetPaginatedKeywords([FromQuery] int skip = 0, int take = 10)
        {
            var pagedKeywords = await _applicationUnitOfWork.GetPagedAsync<Keyword>(skip, take);

			return Ok(pagedKeywords);
        }

		[HttpPost]
		[Permission(PermissionResource.Keyword, CrudActions.Create)]
		[LogAction(nameof(Keyword), nameof(Create))]
		public async Task<IActionResult> Create([FromBody] string word)
        {
            Keyword keyword = new()
            {
                Word = word
            };

			await _genericRepository.CreateAsync(keyword);

			HttpContext.Items.Add("id", keyword.Id);

			return Ok();
        }

		[HttpPatch("{id:int}")]
		[Permission(PermissionResource.Keyword, CrudActions.Update)]
		[LogAction(nameof(Keyword), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] string word, int id)
        {
            var keyword = await _applicationUnitOfWork.FindAsync<Keyword>(id);

			if (keyword == null)
                return NotFound(_localizer["ModelNotFound", nameof(Keyword), id.ToString()].Value);

			await _genericRepository.UpdateAsync(new Keyword { Word = word }, keyword);

			return Ok();
        }

		[HttpDelete("{id:int}")]
		[Permission(PermissionResource.Keyword, CrudActions.Delete)]
		[LogAction(nameof(Keyword), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var keyword = await _applicationUnitOfWork.FindAsync<Keyword>(id);

			if (keyword == null)
                return NotFound(_localizer["ModelNotFound", nameof(Keyword), id.ToString()].Value);

			await _genericRepository.DeleteAsync(keyword);

			return Ok();
        }
    }
}
