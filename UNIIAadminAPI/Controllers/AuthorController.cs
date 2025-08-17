using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MongoDB.Driver;
using System.Net.Mime;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
	[ApiController]
    [Route("api/v1/authors")]
    public class AuthorController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly MongoDbContext _mongoDbContext;
        private readonly IFileEntityService _fileService;
		private readonly IPaginationService _paginationService;
        private readonly IMapper _mapper;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public AuthorController(
            ApplicationContext applicationContext,
            MongoDbContext mongoDbContext,
            IMapper mapper,
            IFileEntityService fileService,
            IPaginationService paginationService,
            IStringLocalizer<ErrorMessages> localizer)
        {
            _applicationContext = applicationContext;
            _mongoDbContext = mongoDbContext;
            _mapper = mapper;
            _fileService=fileService;
            _paginationService=paginationService;
            _localizer = localizer;
        }

		[Permission(PermissionResource.Author, CrudActions.View)]
		[HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
			var author = await _applicationContext.Authors.FindAsync(id);

			if (author == null)
                return NotFound(_localizer["ModelNotFound", nameof(Author), id.ToString()].Value);

            return Ok(author);
        }

		[Permission(PermissionResource.Author, CrudActions.View)]
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

		[Permission(PermissionResource.Author, CrudActions.View)]
		[HttpGet("page")]
        public async Task<IActionResult> GetPagedAuthors([FromQuery] int skip = 0, int take = 10)
        {
            var pagedAuthors = await _paginationService.GetPagedListAsync(_applicationContext.Authors, skip, take);

            return Ok(pagedAuthors);
        }

		[Permission(PermissionResource.Author, CrudActions.Create)]
		[HttpPost]
		[LogAction(nameof(Author), nameof(Create))]
		public async Task<IActionResult> Create([FromForm] Author author, IFormFile? photoFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(_localizer["ModelNotValid"].Value);
            }

            author.Id = 0;

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

		[Permission(PermissionResource.Author, CrudActions.Update)]
		[HttpPatch("{id}")]
		[LogAction(nameof(Author), nameof(Update))]
		public async Task<IActionResult> Update([FromForm] Author author, IFormFile? photoFile, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(_localizer["ModelNotValid"].Value);
            }

            var existedAuthor = await _applicationContext.Authors.FindAsync(id);

            if (existedAuthor == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(Author), id.ToString()].Value);
            }

            _mapper.Map(author, existedAuthor);

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

		[Permission(PermissionResource.Author, CrudActions.Delete)]
		[HttpDelete("{id:int}")]
		[LogAction(nameof(Author), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var author = await _applicationContext.Authors.FindAsync(id);

			if (author == null)
                return NotFound(_localizer["ModelNotFound", nameof(Author), id.ToString()].Value);

            await _fileService.DeleteFileAsync(author.PhotoId, _mongoDbContext.AuthorPhotos);

            _applicationContext.Authors.Remove(author);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}