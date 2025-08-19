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
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("api/v1/publication-languages")]
    public class PublicationLanguageController : ControllerBase
    {
		private readonly IGenericRepository _genericRepository;
		private readonly IApplicationUnitOfWork _applicationUnitOfWork;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public PublicationLanguageController(
			IGenericRepository genericRepository,
			IStringLocalizer<ErrorMessages> localizer,
			IApplicationUnitOfWork applicationUnitOfWork)
		{
			_localizer = localizer;
			_genericRepository = genericRepository;
			_applicationUnitOfWork = applicationUnitOfWork;
		}

		[HttpGet("{id:int}")]
		[Permission(PermissionResource.PublicationLanguadge, CrudActions.View)]
		public async Task<IActionResult> Get(int id)
        {
			var language = await _applicationUnitOfWork.FindAsync<PublicationLanguage>(id);

			if (language == null)
                return NotFound(_localizer["ModelNotFound", nameof(PublicationLanguage), id.ToString()].Value);

            return Ok(language);
        }

        [HttpGet("page")]
		[Permission(PermissionResource.PublicationLanguadge, CrudActions.View)]
		public async Task<IActionResult> GetPaginated([FromQuery] int skip = 0, int take = 10)
        {
			var languages = await _applicationUnitOfWork.GetPagedAsync<PublicationLanguage>(skip, take);

			return Ok(languages);
        }

        [HttpPost]
		[Permission(PermissionResource.PublicationLanguadge, CrudActions.Create)]
		[LogAction(nameof(PublicationLanguage), nameof(Create))]
		public async Task<IActionResult> Create([FromBody] string name)
        {
            PublicationLanguage language = new()
            {
                Name = name
            };

			await _genericRepository.CreateAsync(language);

			HttpContext.Items.Add("id", language.Id);

			return Ok();
        }

        [HttpPatch("{id:int}")]
		[Permission(PermissionResource.PublicationLanguadge, CrudActions.Update)]
		[LogAction(nameof(PublicationLanguage), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] string name, int id)
        {
			var language = await _applicationUnitOfWork.FindAsync<PublicationLanguage>(id);

			if (language == null)
                return NotFound(_localizer["ModelNotFound", nameof(PublicationLanguage), id.ToString()].Value);

			await _genericRepository.UpdateAsync(new PublicationLanguage { Name = name }, language);

			return Ok();
        }

        [HttpDelete("{id:int}")]
		[Permission(PermissionResource.PublicationLanguadge, CrudActions.Delete)]
		[LogAction(nameof(PublicationLanguage), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
			var language = await _applicationUnitOfWork.FindAsync<PublicationLanguage>(id);

			if (language == null)
                return NotFound(_localizer["ModelNotFound", nameof(PublicationLanguage), id.ToString()].Value);

			await _genericRepository.DeleteAsync(language);

			return Ok();
        }
    }
}
