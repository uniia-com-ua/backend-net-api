using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;
using System.Net.Mime;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("api/v1/universities")]
    public class UniversityController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly MongoDbContext _mongoDbContext;
        private readonly IMapper _mapper;
        private readonly IFileEntityService _fileEntityService;
        private readonly IPaginationService _paginationService;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public UniversityController(
            ApplicationContext applicationContext,
            MongoDbContext mongoDbContext,
            IMapper mapper,
            IFileEntityService fileEntityService,
            IPaginationService paginationService,
			IStringLocalizer<ErrorMessages> localizer)
        {
            _applicationContext = applicationContext;
            _mongoDbContext = mongoDbContext;
            _mapper = mapper;
            _fileEntityService = fileEntityService;
            _paginationService = paginationService;
            _localizer = localizer;
        }

        [HttpGet("{id:int}")]
		[Permission(PermissionResource.University, CrudActions.View)]
		public async Task<IActionResult> Get(int id)
        {
            var university = await _applicationContext.Universities.FindAsync(id);

            if (university == null)
                return NotFound(_localizer["ModelNotFound", nameof(University), id.ToString()].Value);

            return Ok(university);
        }

        [HttpGet("{id:int}/photo")]
		[Permission(PermissionResource.University, CrudActions.View)]
		public async Task<IActionResult> GetPicture(int id)
        {
            var photoId = await _applicationContext.Universities
                                                   .Where(u => u.Id == id)
                                                   .Select(u => u.PhotoId)
                                                   .FirstOrDefaultAsync();

            var result = await _fileEntityService.GetFileAsync(photoId, _mongoDbContext.UniversityPhotos);

            if (!result.IsSuccess)
            {
                if (result.Error is InvalidDataException)
                {
                    return BadRequest(result.Error?.Message);
                }

                if (result.Error is ArgumentException || result.Error is KeyNotFoundException)
                {
                    return NotFound(result.Error?.Message);
                }
            }

            return File(result.Value!.File!, MediaTypeNames.Image.Jpeg);
        }

        [HttpGet("{id:int}/small-photo")]
		[Permission(PermissionResource.University, CrudActions.View)]
		public async Task<IActionResult> GetSmallPicture(int id)
        {
            var photoId = await _applicationContext.Universities
                                                   .Where(u => u.Id == id)
                                                   .Select(u => u.SmallPhotoId)
                                                   .FirstOrDefaultAsync();

            var result = await _fileEntityService.GetFileAsync(photoId, _mongoDbContext.UniversityPhotos);

            if (!result.IsSuccess)
            {
                if (result.Error is InvalidDataException)
                {
                    return BadRequest(result.Error?.Message);
                }

                if (result.Error is ArgumentException || result.Error is KeyNotFoundException)
                {
                    return NotFound(result.Error?.Message);
                }
            }

            return File(result.Value!.File!, MediaTypeNames.Image.Jpeg);
        }

        [HttpGet("page")]
		[Permission(PermissionResource.University, CrudActions.View)]
		public async Task<IActionResult> GetPagedUniversities(int skip = 0, int take = 10)
        {
            var universitiesList = await _paginationService.GetPagedListAsync(_applicationContext.Universities, skip, take);

            return Ok(universitiesList);
        }

        [HttpPost]
		[Permission(PermissionResource.University, CrudActions.Create)]
		[LogAction(nameof(University), nameof(Create))]
        public async Task<IActionResult> Create([FromForm] University university, IFormFile? photoFile, IFormFile? smallPhotoFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(_localizer["ModelNotValid"].Value);
            }

            if (photoFile != null)
            {
                var result = await _fileEntityService.SaveFileAsync(photoFile, _mongoDbContext.UniversityPhotos, MediaTypeNames.Image.Jpeg);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

                university.PhotoId = result.Value!.Id.ToString();
            }

            if (smallPhotoFile != null)
            {
                var result = await _fileEntityService.SaveFileAsync(smallPhotoFile, _mongoDbContext.UniversityPhotos, MediaTypeNames.Image.Jpeg);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

                university.SmallPhotoId = result.Value!.Id.ToString();
            }

            await _applicationContext.Universities.AddAsync(university);

            await _applicationContext.SaveChangesAsync();

            HttpContext.Items.Add("id", university.Id);

            return Ok();
        }

        [HttpPatch("{id:int}")]
		[Permission(PermissionResource.University, CrudActions.Update)]
		[LogAction(nameof(University), nameof(Update))]
        public async Task<IActionResult> Update([FromForm] University university, IFormFile? photoFile, IFormFile? smallPhotoFile, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(_localizer["ModelNotValid"].Value);
            }

            var existedUniversity = await _applicationContext.Universities.FindAsync(id);

            if (existedUniversity == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(University), id.ToString()].Value);
            }

            _mapper.Map(university, existedUniversity);

            if (photoFile != null)
            {
                var result = await _fileEntityService.UpdateFileAsync(photoFile, university.PhotoId, _mongoDbContext.UniversityPhotos, MediaTypeNames.Image.Jpeg);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

				existedUniversity.PhotoId = result.Value!.Id.ToString();
            }

            if (smallPhotoFile != null)
            {
                var result = await _fileEntityService.UpdateFileAsync(smallPhotoFile, university.SmallPhotoId, _mongoDbContext.UniversityPhotos, MediaTypeNames.Image.Jpeg);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

				existedUniversity.SmallPhotoId = result.Value!.Id.ToString();
            }

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
		[Permission(PermissionResource.University, CrudActions.Delete)]
		[LogAction(nameof(University), nameof(Delete))]
        public async Task<IActionResult> Delete(int id)
        {
            var university = await _applicationContext.Universities.FindAsync(id);

            if (university == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(University), id.ToString()].Value);
            }

            await _fileEntityService.DeleteFileAsync(university.PhotoId, _mongoDbContext.UniversityPhotos);

            _applicationContext.Universities.Remove(university);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}
