using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UniiaAdmin.Data.Controllers
{
    [ApiController]
    [Route("api/v1/publication-types")]
    public class PublicationTypeController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IPaginationService _paginationService;
        private readonly IStringLocalizer<ErrorMessages> _localizer;

		public PublicationTypeController(
            ApplicationContext applicationContext,
            IPaginationService paginationService,
			IStringLocalizer<ErrorMessages> localizer)
        {
            _applicationContext = applicationContext;
            _paginationService = paginationService;
            _localizer = localizer;
        }

        [HttpGet("{id:int}")]
		[Permission(PermissionResource.PublicationType, CrudActions.View)]
		public async Task<IActionResult> Get(int id)
        {
            var publicationType = await _applicationContext.PublicationTypes.FirstOrDefaultAsync(pt => pt.Id == id);

            if (publicationType == null)
                return NotFound(_localizer["ModelNotFound", nameof(PublicationType), id.ToString()].Value);

            return Ok(publicationType);
        }

        [HttpGet("page")]
		[Permission(PermissionResource.PublicationType, CrudActions.View)]
		public async Task<IActionResult> GetPaginated(int skip = 0, int take = 10)
        {
            var publicationTypes = await _paginationService.GetPagedListAsync(_applicationContext.PublicationTypes, skip, take);

            return Ok(publicationTypes);
        }

        [HttpPost]
		[Permission(PermissionResource.PublicationType, CrudActions.Create)]
		[LogAction(nameof(PublicationType), nameof(Create))]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            if (!ModelState.IsValid)
                return BadRequest(_localizer["ModelNotValid"].Value);

            PublicationType publicationType = new()
            {
                Name = name
            };

            await _applicationContext.PublicationTypes.AddAsync(publicationType);

            await _applicationContext.SaveChangesAsync();

            HttpContext.Items.Add("id", publicationType.Id);

            return Ok();
        }

        [HttpPatch("{id:int}")]
		[Permission(PermissionResource.PublicationType, CrudActions.Update)]
		[LogAction(nameof(PublicationType), nameof(Update))]
        public async Task<IActionResult> Update([FromBody] string name, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(_localizer["ModelNotValid"].Value);

            var publicationType = await _applicationContext.PublicationTypes.FirstOrDefaultAsync(pt => pt.Id == id);

            if (publicationType == null)
                return NotFound(_localizer["ModelNotFound", nameof(PublicationType), id.ToString()].Value);

            publicationType.Name = name;

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
		[Permission(PermissionResource.PublicationType, CrudActions.Delete)]
		[LogAction(nameof(PublicationType), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var publicationType = await _applicationContext.PublicationTypes.FirstOrDefaultAsync(pt => pt.Id == id);

            if (publicationType == null)
                return NotFound(_localizer["ModelNotFound", nameof(PublicationType), id.ToString()].Value);

            _applicationContext.Remove(publicationType);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}
