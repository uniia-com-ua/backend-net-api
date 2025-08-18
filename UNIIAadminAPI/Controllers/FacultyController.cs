using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Resources;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("api/v1/faculties")]
    public class FacultyController : ControllerBase
    {
		private readonly IGenericRepository _genericRepository;
		private readonly IQueryRepository _queryRepository;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public FacultyController(
			IGenericRepository genericRepository,
			IStringLocalizer<ErrorMessages> localizer,
			IQueryRepository queryRepository)
		{
			_localizer = localizer;
			_genericRepository = genericRepository;
			_queryRepository = queryRepository;
		}

		[HttpGet("{id:int}")]
		[Permission(PermissionResource.Faculty, CrudActions.View)]
        public async Task<IActionResult> Get(int id)
        {
            var faculty = await _queryRepository.GetByIdAsync<Faculty>(id);

            if (faculty == null)
                return NotFound(_localizer["ModelNotFound", nameof(Faculty), id.ToString()].Value);

            return Ok(faculty);
        }

		[HttpGet("page")]
		[Permission(PermissionResource.Faculty, CrudActions.View)]
        public async Task<IActionResult> GetPaginatedFacultied([FromQuery] int skip = 0, int take = 10)
        {
            var pagedFaculties = await _queryRepository.GetPagedAsync<Faculty>(skip, take);

            return Ok(pagedFaculties);
        }

		[HttpPost]
		[Permission(PermissionResource.Faculty, CrudActions.Create)]
        [LogAction(nameof(Faculty), nameof(Create))]
        public async Task<IActionResult> Create([FromBody] Faculty faculty)
        {
            var isUniExist = await _queryRepository.AnyAsync<University>(faculty.UniversityId);

            if (!isUniExist)
                return NotFound(_localizer["ModelNotFound", nameof(University), faculty.UniversityId.ToString()].Value);

            await _genericRepository.CreateAsync(faculty);

            HttpContext.Items.Add("id", faculty.Id);

            return Ok();
        }

		[HttpPatch("{id:int}")]
		[Permission(PermissionResource.Faculty, CrudActions.Update)]
		[LogAction(nameof(Faculty), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] Faculty faculty, int id)
        {
            var existedFaculty = await _queryRepository.GetByIdAsync<Faculty>(id);

			if (existedFaculty == null)
                return NotFound(_localizer["ModelNotFound", nameof(Faculty), id.ToString()].Value);

			var isUniExist = await _queryRepository.AnyAsync<University>(faculty.UniversityId);

			if (!isUniExist)
				return NotFound(_localizer["ModelNotFound", nameof(University), faculty.UniversityId.ToString()].Value);

			await _genericRepository.UpdateAsync(faculty, existedFaculty);

            return Ok();
        }

		[HttpDelete("{id:int}")]
		[Permission(PermissionResource.Faculty, CrudActions.Delete)]
		[LogAction(nameof(Faculty), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
			var faculty = await _queryRepository.GetByIdAsync<Faculty>(id);

			if (faculty == null)
                return NotFound(_localizer["ModelNotFound", nameof(Faculty), id.ToString()].Value);

			await _genericRepository.DeleteAsync(faculty);

			return Ok();
        }
    }
}