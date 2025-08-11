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
    [Route("api/v1/authors")]
    public class AuthorController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly MongoDbContext _mongoDbContext;
        private readonly IFileEntityService _fileService;
		private readonly IPaginationService _paginationService;
        private readonly IMapper _mapper;

        public AuthorController(
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
            _paginationService=paginationService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var author = await _applicationContext.Authors.FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Author), id.ToString()));

            var result = _mapper.Map<AuthorDto>(author);

            return Ok(result);
        }

        [HttpGet("{id:int}/photo")]
        public async Task<IActionResult> GetPicture(int id)
        {
            var photoId = await _applicationContext.Authors
                                                   .Where(a => a.Id == id)
                                                   .Select(a => a.PhotoId)
                                                   .FirstOrDefaultAsync();

            var result = await _fileService.GetFileAsync(photoId, _mongoDbContext.AuthorPhotos);

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
        public async Task<IActionResult> GetPagedAuthors([FromQuery] int skip = 0, int take = 10)
        {
            var pagedAuthors = await _paginationService.GetPagedListAsync(_applicationContext.Authors, skip, take);

            var resultList = pagedAuthors.Select(a => _mapper.Map<AuthorDto>(a));

            return Ok(resultList);
        }

        [HttpPost]
		[LogAction(nameof(Author), nameof(Create))]
		public async Task<IActionResult> Create([FromForm] AuthorDto authorDto, IFormFile? photoFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorMessages.ModelNotValid);
            }

            var author = _mapper.Map<Author>(authorDto);

            if (photoFile != null)
            {
                var result = await _fileService.SaveFileAsync(photoFile, _mongoDbContext.AuthorPhotos, MediaTypeNames.Image.Jpeg);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

                author.PhotoId = result.Value!.Id.ToString();
            }

            await _applicationContext.Authors.AddAsync(author);

            await _applicationContext.SaveChangesAsync();

            HttpContext.Items.Add("id", author.Id);

            return Ok();
        }

        [HttpPatch("{id}")]
		[LogAction(nameof(Author), nameof(Update))]
		public async Task<IActionResult> Update([FromForm] AuthorDto authorDto, IFormFile? photoFile, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorMessages.ModelNotValid);
            }

            var author = await _applicationContext.Authors.FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                return NotFound(ErrorMessages.ModelNotFound(nameof(Author), id.ToString()));
            }

            author.Update(authorDto);

            if (photoFile != null)
            {
                var result = await _fileService.UpdateFileAsync(photoFile, author.PhotoId, _mongoDbContext.AuthorPhotos, MediaTypeNames.Image.Jpeg);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

                author.PhotoId = result.Value!.Id.ToString();
            }

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
		[LogAction(nameof(Author), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var author = await _applicationContext.Authors.FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Author), id.ToString()));

            await _fileService.DeleteFileAsync(author.PhotoId, _mongoDbContext.AuthorPhotos);

            _applicationContext.Authors.Remove(author);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}