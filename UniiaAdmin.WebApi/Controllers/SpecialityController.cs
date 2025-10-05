using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net.Mime;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UniiaAdmin.WebApi.Controllers
{
	[ApiController]
    [Route("api/v1/specialities")]
    public class SpecialityController : ControllerBase
    {
		private readonly IApplicationUnitOfWork _applicationUnitOfWork;
		private readonly IGenericRepository _genericRepository;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public SpecialityController(IStringLocalizer<ErrorMessages> localizer,
			IApplicationUnitOfWork applicationUnitOfWork,
			IGenericRepository genericRepository)
		{
			_localizer = localizer;
			_applicationUnitOfWork = applicationUnitOfWork;
			_genericRepository = genericRepository;
		}

		[Permission(PermissionResource.Speciality, CrudActions.View)]
		[HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var speciality = await _applicationUnitOfWork.FindAsync<Specialty>(id);

			if (speciality == null)
                return NotFound(_localizer["ModelNotFound", nameof(Specialty), id.ToString()].Value);

            return Ok(speciality);
        }

		[Permission(PermissionResource.Speciality, CrudActions.View)]
		[HttpGet("page")]
        public async Task<IActionResult> GetPagedAuthors([FromQuery] string? sort = null, int skip = 0, int take = 10)
        {
			var pagedSpecialties = await _applicationUnitOfWork.GetPagedAsync<Specialty>(skip, take, sort);

			return Ok(pagedSpecialties);
        }

		[Permission(PermissionResource.Speciality, CrudActions.Create)]
		[HttpPost]
		[LogAction(nameof(Specialty), nameof(Create))]
		public async Task<IActionResult> Create([FromBody] Specialty specialty)
        {
			await _genericRepository.CreateAsync(specialty);

			HttpContext.Items.Add("id", specialty.Id);

            return Ok();
        }

		[Permission(PermissionResource.Speciality, CrudActions.Update)]
		[HttpPatch("{id}")]
		[LogAction(nameof(Specialty), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] Specialty specialty, int id)
        {
            var existedSpecialty = await _applicationUnitOfWork.FindAsync<Specialty>(id);

			if (existedSpecialty == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(Specialty), id.ToString()].Value);
            }

			await _genericRepository.UpdateAsync(specialty, existedSpecialty);

            return Ok();
        }

		[Permission(PermissionResource.Speciality, CrudActions.Delete)]
		[HttpDelete("{id:int}")]
		[LogAction(nameof(Specialty), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
			var specialty = await _applicationUnitOfWork.FindAsync<Specialty>(id);

			if (specialty == null)
                return NotFound(_localizer["ModelNotFound", nameof(Specialty), id.ToString()].Value);

            await _genericRepository.DeleteAsync(specialty);

            return Ok();
        }
    }
}