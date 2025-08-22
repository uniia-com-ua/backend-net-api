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
    [Route("api/v1/authors")]
    public class AuthorController : ControllerBase
    {
        private readonly IPhotoRepository _authorRepository;
        private readonly IPhotoProvider _photoProvider;
		private readonly IApplicationUnitOfWork _applicationUnitOfWork;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public AuthorController(
            IPhotoRepository authorRepository,
            IStringLocalizer<ErrorMessages> localizer,
            IPhotoProvider photoProvider,
			IApplicationUnitOfWork applicationUnitOfWork)
        {
            _localizer = localizer;
            _authorRepository = authorRepository;
            _photoProvider = photoProvider;
			_applicationUnitOfWork = applicationUnitOfWork;
		}

		[Permission(PermissionResource.Author, CrudActions.View)]
		[HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var author = await _applicationUnitOfWork.FindAsync<Author>(id);

			if (author == null)
                return NotFound(_localizer["ModelNotFound", nameof(Author), id.ToString()].Value);

            return Ok(author);
        }

		[Permission(PermissionResource.Author, CrudActions.View)]
		[HttpGet("{id:int}/photo")]
        public async Task<IActionResult> GetPicture(int id)
        {
			var result = await _photoProvider.GetPhotoAsync<Author, AuthorPhoto>(id);

			if (!result.IsSuccess)
            {
                if (result.Error is InvalidDataException)
                {
                    return BadRequest(result.Error?.Message);
                } 
                else
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
			var pagedAuthors = await _applicationUnitOfWork.GetPagedAsync<Author>(skip, take);

			return Ok(pagedAuthors);
        }

		[Permission(PermissionResource.Author, CrudActions.Create)]
		[HttpPost]
		[LogAction(nameof(Author), nameof(Create))]
		public async Task<IActionResult> Create([FromForm] Author author, IFormFile? photoFile)
        {
			var result = await _authorRepository.CreateAsync<Author, AuthorPhoto>(author, photoFile);

			if (!result.IsSuccess)
			{
				return BadRequest(result.Error?.Message);
			}

            HttpContext.Items.Add("id", author.Id);

            return Ok();
        }

		[Permission(PermissionResource.Author, CrudActions.Update)]
		[HttpPatch("{id}")]
		[LogAction(nameof(Author), nameof(Update))]
		public async Task<IActionResult> Update([FromForm] Author author, IFormFile? photoFile, int id)
        {
            var existedAuthor = await _applicationUnitOfWork.FindAsync<Author>(id);

			if (existedAuthor == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(Author), id.ToString()].Value);
            }

			var result = await _authorRepository.UpdateAsync<Author, AuthorPhoto>(author, existedAuthor, photoFile);

			if (!result.IsSuccess)
			{
				return BadRequest(result.Error?.Message);
			}

            return Ok();
        }

		[Permission(PermissionResource.Author, CrudActions.Delete)]
		[HttpDelete("{id:int}")]
		[LogAction(nameof(Author), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
			var author = await _applicationUnitOfWork.FindAsync<Author>(id);

			if (author == null)
                return NotFound(_localizer["ModelNotFound", nameof(Author), id.ToString()].Value);

            await _authorRepository.DeleteAsync<Author, AuthorPhoto>(author);

            return Ok();
        }
    }
}