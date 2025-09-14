using AutoMapper;
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
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UniiaAdmin.WebApi.Controllers
{
	[ApiController]
    [Route("api/v1/publications")]
    public class PublicationController : ControllerBase
    {
		private readonly IFileRepository _fileRepository;
		private readonly IApplicationUnitOfWork _applicationUnitOfWork;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public PublicationController(
			IFileRepository fileRepository,
			IStringLocalizer<ErrorMessages> localizer,
			IApplicationUnitOfWork applicationUnitOfWork)
		{
			_localizer = localizer;
            _fileRepository = fileRepository;
			_applicationUnitOfWork = applicationUnitOfWork;
		}

		[HttpGet("{id:int}")]
		[Permission(PermissionResource.Publication, CrudActions.View)]
		public async Task<IActionResult> Get(int id)
        {
            var publication = await _applicationUnitOfWork.GetByIdWithIncludesAsync<Publication>(p => p.Id == id,
                p => p.Keywords!,
                p => p.Authors!,
                p => p.Subjects!,
                p => p.PublicationType!,
                p => p.Language!);

			if (publication == null)
                return NotFound(_localizer["ModelNotFound", nameof(Publication), id.ToString()].Value);

            return Ok(publication);
        }

        [HttpGet("{id:int}/file")]
		[Permission(PermissionResource.Publication, CrudActions.View)]
		public async Task<IActionResult> GetFile(int id)
        {
            var result = await _fileRepository.GetFileAsync<Publication, PublicationFile>(id);

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

            return File(result.Value!.File!, MediaTypeNames.Application.Pdf);
        }

        [HttpGet("page")]
		[Permission(PermissionResource.Publication, CrudActions.View)]
		public async Task<IActionResult> GetPagedPublications([FromQuery] int skip = 0, int take = 10)
        {
            var publications = await _applicationUnitOfWork.GetPagedAsync<Publication>(skip, take);

            return Ok(publications);
        }

        [HttpPost]
		[Permission(PermissionResource.Publication, CrudActions.Create)]
		[LogAction(nameof(Publication), nameof(Create))]
		public async Task<IActionResult> Create([FromForm] Publication publication, PublicationUpdateDto publicationUpdateDto)
        {
			var isTypeExist = await _applicationUnitOfWork.AnyAsync<PublicationType>(publication.PublicationTypeId);

			if (!isTypeExist)
				return NotFound(_localizer["ModelNotFound", nameof(PublicationType), publication.PublicationTypeId.ToString()].Value);

			var isLangExist = await _applicationUnitOfWork.AnyAsync<PublicationLanguage>(publication.PublicationLanguageId);

			if (!isLangExist)
				return NotFound(_localizer["ModelNotFound", nameof(PublicationLanguage), publication.PublicationLanguageId.ToString()].Value);

			publication.Subjects = await _applicationUnitOfWork.GetByIdsAsync<Subject>(publicationUpdateDto.Subjects);

			publication.Authors = await _applicationUnitOfWork.GetByIdsAsync<Author>(publicationUpdateDto.Authors);

			publication.Keywords = await _applicationUnitOfWork.GetByIdsAsync<Keyword>(publicationUpdateDto.Keywords);

			publication.CreatedDate = DateTime.UtcNow;

			publication.LastModifiedDate = DateTime.UtcNow;

			await _fileRepository.CreateAsync<Publication, PublicationFile>(publication, publicationUpdateDto.File);

			HttpContext.Items.Add("id", publication.Id);

			return Ok();
        }

        [HttpPatch("{id:int}")]
		[Permission(PermissionResource.Publication, CrudActions.Update)]
		[LogAction(nameof(Publication), nameof(Update))]
		public async Task<IActionResult> Update([FromForm] Publication publication, PublicationUpdateDto publicationUpdateDto, int id)
        {
            var existedPublication = await _applicationUnitOfWork.FindAsync<Publication>(id);

			if (existedPublication == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(Publication), id.ToString()].Value);
            }

			var isTypeExist = await _applicationUnitOfWork.AnyAsync<PublicationType>(publication.PublicationTypeId);

			if (!isTypeExist)
				return NotFound(_localizer["ModelNotFound", nameof(PublicationType), publication.PublicationTypeId.ToString()].Value);

			var isLangExist = await _applicationUnitOfWork.AnyAsync<PublicationLanguage>(publication.PublicationLanguageId);

			if (!isLangExist)
				return NotFound(_localizer["ModelNotFound", nameof(PublicationLanguage), publication.PublicationLanguageId.ToString()].Value);

			existedPublication.Subjects = await _applicationUnitOfWork.GetByIdsAsync<Subject>(publicationUpdateDto.Subjects) ?? existedPublication.Subjects;

			existedPublication.Authors = await _applicationUnitOfWork.GetByIdsAsync<Author>(publicationUpdateDto.Authors) ?? existedPublication.Authors;

			existedPublication.Keywords = await _applicationUnitOfWork.GetByIdsAsync<Keyword>(publicationUpdateDto.Keywords) ?? existedPublication.Keywords;

			existedPublication.LastModifiedDate = DateTime.UtcNow;

			var result = await _fileRepository.UpdateAsync<Publication, PublicationFile>(publication, existedPublication, publicationUpdateDto.File);

			if (!result.IsSuccess)
			{
				return BadRequest(result.Error?.Message);
			}

            return Ok();
        }

        [HttpDelete("{id:int}")]
		[Permission(PermissionResource.Publication, CrudActions.Delete)]
		[LogAction(nameof(Publication), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
			var publication = await _applicationUnitOfWork.FindAsync<Publication>(id);

            if (publication == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(Publication), id.ToString()].Value);
            }

			await _fileRepository.DeleteAsync<Publication, PublicationFile>(publication);

            return Ok();
        }
    }
}