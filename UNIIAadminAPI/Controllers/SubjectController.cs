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

namespace UniiaAdmin.WebApi.Controllers
{
	[ApiController]
    [Route("api/v1/subjects")]
    public class SubjectController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IPaginationService _paginationService;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public SubjectController(
            ApplicationContext applicationContext,
            IPaginationService paginationService,
			IStringLocalizer<ErrorMessages> localizer)
        {
            _applicationContext = applicationContext;
            _paginationService = paginationService;
            _localizer = localizer;
        }

        [HttpGet("{id:int}")]
		[Permission(PermissionResource.Subject, CrudActions.View)]
		public async Task<IActionResult> Get(int id)
        {
            var subject = await _applicationContext.Subjects.FindAsync(id);

            if (subject == null)
                return NotFound(_localizer["ModelNotFound", nameof(Subject), id.ToString()].Value);

            return Ok(subject);
        }

        [HttpGet("page")]
		[Permission(PermissionResource.Subject, CrudActions.View)]
		public async Task<IActionResult> GetPaginated([FromQuery] int skip = 0, int take = 10)
        {
            var subjects = await _paginationService.GetPagedListAsync(_applicationContext.Subjects, skip, take);

            return Ok(subjects);
        }

        [HttpPost]
		[Permission(PermissionResource.Subject, CrudActions.Create)]
		[LogAction(nameof(Subject), nameof(Create))]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            if (!ModelState.IsValid)
                return BadRequest(_localizer["ModelNotValid"].Value);

            Subject subject = new()
            {
                Name = name
            };

            await _applicationContext.Subjects.AddAsync(subject);

            await _applicationContext.SaveChangesAsync();

			HttpContext.Items.Add("id", subject.Id);

			return Ok();
        }

        [HttpPatch("{id:int}")]
		[Permission(PermissionResource.Subject, CrudActions.Update)]
		[LogAction(nameof(Subject), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] string name, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(_localizer["ModelNotValid"].Value);

            var subject = await _applicationContext.Subjects.FindAsync(id);

			if (subject == null)
                return NotFound(_localizer["ModelNotFound", nameof(Subject), id.ToString()].Value);

            subject.Name = name;

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
		[Permission(PermissionResource.Subject, CrudActions.Delete)]
		[LogAction(nameof(Subject), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var subject = await _applicationContext.Subjects.FindAsync(id);

            if (subject == null)
                return NotFound(_localizer["ModelNotFound", nameof(Subject), id.ToString()].Value);

            _applicationContext.Remove(subject);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}
