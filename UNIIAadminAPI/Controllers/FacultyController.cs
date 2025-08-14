using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("api/v1/faculties")]
    public class FacultyController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IMapper _mapper;
        private readonly IPaginationService _paginationService;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public FacultyController(
            ApplicationContext applicationContext,
            IMapper mapper,
            IPaginationService paginationService,
            IStringLocalizer<ErrorMessages> localizer)
        {
            _applicationContext = applicationContext;
            _mapper = mapper;
            _paginationService = paginationService;
            _localizer = localizer;
        }

		[HttpGet("{id:int}")]
		[Permission(PermissionResource.Faculty, CrudActions.View)]
        public async Task<IActionResult> Get(int id)
        {
            var faculty = await _applicationContext.Faculties.FirstOrDefaultAsync(a => a.Id == id);

            if (faculty == null)
                return NotFound(_localizer["ModelNotFound", nameof(Faculty), id.ToString()].Value);

            var result = _mapper.Map<FacultyDto>(faculty);

            return Ok(result);
        }

		[HttpGet("page")]
		[Permission(PermissionResource.Faculty, CrudActions.View)]
        public async Task<IActionResult> GetPaginatedFacultied(int skip = 0, int take = 10)
        {
            var pagedFaculties = await _paginationService.GetPagedListAsync(_applicationContext.Faculties, skip, take);

            var result = pagedFaculties.Select(f => _mapper.Map<FacultyDto>(f));

            return Ok(result);
        }

		[HttpPost]
		[Permission(PermissionResource.Faculty, CrudActions.Create)]
        [LogAction(nameof(Faculty), nameof(Create))]
        public async Task<IActionResult> Create([FromForm] FacultyDto facultyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(_localizer["ModelNotValid"].Value);
            }

            Faculty faculty = _mapper.Map<Faculty>(facultyDto);

            var isUniExist = await _applicationContext.Universities.AnyAsync(u => u.Id == faculty.UniversityId);

            if (!isUniExist)
                return NotFound(_localizer["ModelNotFound", nameof(University), faculty.UniversityId.ToString()].Value);

            await _applicationContext.Faculties.AddAsync(faculty);

            await _applicationContext.SaveChangesAsync();

            HttpContext.Items.Add("id", faculty.Id);

            return Ok();
        }

		[HttpPatch("{id:int}")]
		[Permission(PermissionResource.Faculty, CrudActions.Update)]
		[LogAction(nameof(Faculty), nameof(Update))]
		public async Task<IActionResult> Update(FacultyDto facultyDto, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(_localizer["ModelNotValid"].Value);
            }

            var faculty = await _applicationContext.Faculties.FirstOrDefaultAsync(a => a.Id == id);

            if (faculty == null)
                return NotFound(_localizer["ModelNotFound", nameof(Faculty), id.ToString()].Value);

            faculty.Update(facultyDto);

            var isUniExist = await _applicationContext.Universities.AnyAsync(u => u.Id == faculty.UniversityId);

            if (!isUniExist)
                return NotFound(_localizer["ModelNotFound", nameof(University), faculty.UniversityId.ToString()].Value);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

		[HttpDelete("{id:int}")]
		[Permission(PermissionResource.Faculty, CrudActions.Delete)]
		[LogAction(nameof(Faculty), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var faculty = await _applicationContext.Faculties.FirstOrDefaultAsync(a => a.Id == id);

            if (faculty == null)
                return NotFound(_localizer["ModelNotFound", nameof(Faculty), id.ToString()].Value);

            _applicationContext.Remove(faculty);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}