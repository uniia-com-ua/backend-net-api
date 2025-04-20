using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Constants;
using UniiaAdmin.WebApi.Helpers;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
	[Authorize]
	[ApiController]
    [Route("faculties")]
    public class FacultyController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IMapper _mapper;

        public FacultyController(ApplicationContext applicationContext,IMapper mapper)
        {
            _applicationContext = applicationContext;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var faculty = await _applicationContext.Faculties.FirstOrDefaultAsync(a => a.Id == id);

            if (faculty == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Faculty), id.ToString()));

            var result = _mapper.Map<FacultyDto>(faculty);

            return Ok(result);
        }

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPaginatedFacultied(int skip, int take)
        {
            var pagedFaculties = await PaginationHelper.GetPagedListAsync(_applicationContext.Faculties, skip, take);

            var result = pagedFaculties.Select(f => _mapper.Map<FacultyDto>(f));

            return Ok(result);
        }

        [HttpPost]
		[LogAction(nameof(Faculty), nameof(Create))]
		public async Task<IActionResult> Create([FromForm] FacultyDto facultyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorMessages.ModelNotValid);
            }

            Faculty faculty = _mapper.Map<Faculty>(facultyDto);

            var isUniExist = await _applicationContext.Universities.AnyAsync(u => u.Id == faculty.UniversityId);

            if (!isUniExist)
                return NotFound(ErrorMessages.ModelNotFound(nameof(University), faculty.UniversityId.ToString()));

            await _applicationContext.Faculties.AddAsync(faculty);

            await _applicationContext.SaveChangesAsync();

			HttpContext.Items.Add("id", faculty.Id);

			return Ok();
        }

        [HttpPatch]
        [Route("{id}")]
		[LogAction(nameof(Faculty), nameof(Update))]
		public async Task<IActionResult> Update(FacultyDto facultyDto, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorMessages.ModelNotValid);
            }

            var faculty = await _applicationContext.Faculties.FirstOrDefaultAsync(a => a.Id == id);

            if (faculty == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Faculty), id.ToString()));

            faculty.Update(facultyDto);

            var isUniExist = await _applicationContext.Universities.AnyAsync(u => u.Id == faculty.UniversityId);

            if (!isUniExist)
                return NotFound(ErrorMessages.ModelNotFound(nameof(University), faculty.UniversityId.ToString()));

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
		[LogAction(nameof(Faculty), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var faculty = await _applicationContext.Faculties.FirstOrDefaultAsync(a => a.Id == id);

            if (faculty == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Faculty), id.ToString()));

            _applicationContext.Remove(faculty);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}