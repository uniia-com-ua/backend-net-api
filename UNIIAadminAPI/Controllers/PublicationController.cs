using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Net.Mime;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Constants;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
	[Authorize]
	[ApiController]
    [Route("api/v1/publications")]
    public class PublicationController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly MongoDbContext _mongoDbContext;
        private readonly IFileEntityService _fileService;
        private readonly IMapper _mapper;
        private readonly IPaginationService _paginationService;

        public PublicationController(
            ApplicationContext applicationContext,
            MongoDbContext mongoDbContext,
            IMapper mapper,
            IFileEntityService fileService,
            IPaginationService paginationService)
        {
            _applicationContext = applicationContext;
            _mongoDbContext = mongoDbContext;
            _mapper = mapper;
            _fileService=fileService;
            _paginationService = paginationService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var publication = await _applicationContext.Publications.FirstOrDefaultAsync(a => a.Id == id);

            if (publication == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Publication), id.ToString()));

            var result = _mapper.Map<PublicationDto>(publication);

            return Ok(result);
        }

        [HttpGet]
        [Route("{id}/file")]
        public async Task<IActionResult> GetFile(int id)
        {
            var fileId = await _applicationContext.Publications
                                                   .Where(a => a.Id == id)
                                                   .Select(a => a.FileId)
                                                   .FirstOrDefaultAsync();

            var result = await _fileService.GetFileAsync(fileId, _mongoDbContext.PublicationFiles);

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

            return File(result.Value!.File!, MediaTypeNames.Application.Pdf);
        }

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPagedPublications(int skip = 0, int take = 10)
        {
            var publications = await _paginationService.GetPagedListAsync(_applicationContext.Universities, skip, take);

            var resultList = publications.Select(a => _mapper.Map<PublicationDto>(a));

            return Ok(resultList);
        }

        [HttpPost]
		[LogAction(nameof(Publication), nameof(Create))]
		public async Task<IActionResult> Create([FromForm] PublicationDto publicationDto, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorMessages.ModelNotValid);
            }

            var publication = _mapper.Map<Publication>(publicationDto);

            if (file != null)
            {
                var result = await _fileService.SaveFileAsync(file, _mongoDbContext.PublicationFiles, MediaTypeNames.Application.Pdf);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

                publication.FileId = result.Value!.Id.ToString();
            }

            await _applicationContext.Publications.AddAsync(publication);

            await _applicationContext.SaveChangesAsync();

			HttpContext.Items.Add("id", publication.Id);

			return Ok();
        }

        [HttpPatch]
        [Route("{id}")]
		[LogAction(nameof(Publication), nameof(Update))]
		public async Task<IActionResult> Update([FromForm] PublicationDto publicationDto, IFormFile? file, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorMessages.ModelNotValid);
            }

            var publication = await _applicationContext.Publications.FirstOrDefaultAsync(a => a.Id == id);

            if (publication == null)
            {
                return NotFound(ErrorMessages.ModelNotFound(nameof(Author), id.ToString()));
            }

            publication.Update(publicationDto);

            if (file != null)
            {
                var result = await _fileService.UpdateFileAsync(file, publication.FileId, _mongoDbContext.PublicationFiles, MediaTypeNames.Image.Jpeg);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

                publication.FileId = result.Value!.Id.ToString();
            }

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
		[LogAction(nameof(Publication), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var publication = await _applicationContext.Publications.FirstOrDefaultAsync(a => a.Id == id);

            if (publication == null)
            {
                return NotFound(ErrorMessages.ModelNotFound(nameof(Publication), id.ToString()));
            }

            await _fileService.DeleteFileAsync(publication.FileId, _mongoDbContext.PublicationFiles);

            _applicationContext.Publications.Remove(publication);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}