using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Net.Mime;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Constants;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
	[Authorize]
	[ApiController]
    [Route("api/v1/universities")]
    public class UniversityController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly MongoDbContext _mongoDbContext;
        private readonly IMapper _mapper;
        private readonly IFileEntityService _fileEntityService;
        private readonly IPaginationService _paginationService;

        public UniversityController(
            ApplicationContext applicationContext,
            MongoDbContext mongoDbContext,
            IMapper mapper,
            IFileEntityService fileEntityService,
            IPaginationService paginationService)
        {
            _applicationContext = applicationContext;
            _mongoDbContext = mongoDbContext;
            _mapper = mapper;
            _fileEntityService = fileEntityService;
            _paginationService = paginationService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var university = await _applicationContext.Universities.FirstOrDefaultAsync(u => u.Id == id);

            if (university == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(University), id.ToString()));

            var result = _mapper.Map<UniversityDto>(university);

            return Ok(result);
        }

        [HttpGet]
        [Route("{id}/photo")]
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

        [HttpGet]
        [Route("{id}/small-photo")]
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

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPagedUniversities(int skip, int take)
        {
            var universitiesList = await _paginationService.GetPagedListAsync(_applicationContext.Universities, skip, take);

            var resultList = universitiesList.Select(u => _mapper.Map<UniversityDto>(u));

            return Ok(resultList);
        }

        [HttpPost]
		[LogAction(nameof(University), nameof(Create))]
		public async Task<IActionResult> Create([FromForm] UniversityDto universityDto, IFormFile? photoFile, IFormFile? smallPhotoFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorMessages.ModelNotValid);
            }

            var university = _mapper.Map<University>(universityDto);

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

        [HttpPatch]
        [Route("{id}")]
		[LogAction(nameof(University), nameof(Update))]
		public async Task<IActionResult> Update([FromForm] UniversityDto universityDto, IFormFile? photoFile, IFormFile? smallPhotoFile, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorMessages.ModelNotValid);
            }

            var university = await _applicationContext.Universities.FirstOrDefaultAsync(u => u.Id == id);

            if (university == null)
            {
                return NotFound(ErrorMessages.ModelNotFound(nameof(University), id.ToString()));
            }

            university.Update(universityDto);

            if (photoFile != null)
            {
                var result = await _fileEntityService.UpdateFileAsync(photoFile, university.PhotoId, _mongoDbContext.UniversityPhotos, MediaTypeNames.Image.Jpeg);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

                university.PhotoId = result.Value!.Id.ToString();
            }

            if (smallPhotoFile != null)
            {
                var result = await _fileEntityService.UpdateFileAsync(smallPhotoFile, university.SmallPhotoId, _mongoDbContext.UniversityPhotos, MediaTypeNames.Image.Jpeg);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

                university.PhotoId = result.Value!.Id.ToString();
            }

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
		[LogAction(nameof(University), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var university = await _applicationContext.Universities.FirstOrDefaultAsync(u => u.Id == id);

            if (university == null)
            {
                return NotFound(ErrorMessages.ModelNotFound(nameof(University), id.ToString()));
            }

            await _fileEntityService.DeleteFileAsync(university.PhotoId, _mongoDbContext.UniversityPhotos);

            _applicationContext.Universities.Remove(university);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}
