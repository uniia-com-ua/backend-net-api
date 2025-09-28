using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;

namespace UniiaAdmin.WebApi.Controllers
{
	[ApiController]
    [Route("api/v1/subjects")]
    public class SubjectController : ControllerBase
    {
		private readonly IGenericRepository _genericRepository;
		private readonly IApplicationUnitOfWork _applicationUnitOfWork;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public SubjectController(
			IGenericRepository genericRepository,
			IStringLocalizer<ErrorMessages> localizer,
			IApplicationUnitOfWork applicationUnitOfWork)
		{
			_localizer = localizer;
			_genericRepository = genericRepository;
			_applicationUnitOfWork = applicationUnitOfWork;
		}

		[HttpGet("{id:int}")]
		[Permission(PermissionResource.Subject, CrudActions.View)]
		public async Task<IActionResult> Get(int id)
        {
			var subject = await _applicationUnitOfWork.FindAsync<Subject>(id);

			if (subject == null)
                return NotFound(_localizer["ModelNotFound", nameof(Subject), id.ToString()].Value);

            return Ok(subject);
        }

        [HttpGet("page")]
		[Permission(PermissionResource.Subject, CrudActions.View)]
		public async Task<IActionResult> GetPaginated([FromQuery] int skip = 0, int take = 10)
        {
			var subjects = await _applicationUnitOfWork.GetPagedAsync<Subject>(skip, take);

			return Ok(subjects);
        }

        [HttpPost]
		[Permission(PermissionResource.Subject, CrudActions.Create)]
		[LogAction(nameof(Subject), nameof(Create))]
        public async Task<IActionResult> Create([FromBody] Subject subject)
        {
			if (subject.SpecialtyId != null)
			{
				var isSpecialityExist = await _applicationUnitOfWork.AnyAsync<Specialty>((int)subject.SpecialtyId);

				if (!isSpecialityExist)
					return NotFound(_localizer["ModelNotFound", nameof(Specialty), ((int)subject.SpecialtyId).ToString()].Value);
			}

			await _genericRepository.CreateAsync(subject);

			HttpContext.Items.Add("id", subject.Id);

			return Ok();
        }

        [HttpPatch("{id:int}")]
		[Permission(PermissionResource.Subject, CrudActions.Update)]
		[LogAction(nameof(Subject), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] Subject subject, int id)
        {
			var existedSubject = await _applicationUnitOfWork.FindAsync<Subject>(id);

			if (existedSubject == null)
                return NotFound(_localizer["ModelNotFound", nameof(Subject), id.ToString()].Value);

			if(subject.SpecialtyId != null)
			{
				var isSpecialityExist = await _applicationUnitOfWork.AnyAsync<Specialty>((int)subject.SpecialtyId);

				if (!isSpecialityExist)
					return NotFound(_localizer["ModelNotFound", nameof(Specialty), ((int)subject.SpecialtyId).ToString()].Value);
			}

			await _genericRepository.UpdateAsync(subject, existedSubject);

			return Ok();
        }

        [HttpDelete("{id:int}")]
		[Permission(PermissionResource.Subject, CrudActions.Delete)]
		[LogAction(nameof(Subject), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
			var subject = await _applicationUnitOfWork.FindAsync<Subject>(id);

			if (subject == null)
                return NotFound(_localizer["ModelNotFound", nameof(Subject), id.ToString()].Value);

			await _genericRepository.DeleteAsync(subject);

			return Ok();
        }
    }
}
