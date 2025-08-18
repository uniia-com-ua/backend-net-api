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
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
	[ApiController]
    [Route("api/v1/publications")]
    public class PublicationController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly MongoDbContext _mongoDbContext;
        private readonly IFileEntityService _fileService;
		private readonly IEntityQueryService _entityQueryService;
		private readonly IMapper _mapper;
        private readonly IPaginationService _paginationService;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public PublicationController(
            ApplicationContext applicationContext,
            MongoDbContext mongoDbContext,
            IMapper mapper,
            IFileEntityService fileService,
            IPaginationService paginationService,
			IStringLocalizer<ErrorMessages> localizer,
            IEntityQueryService entityQueryService)
        {
            _applicationContext = applicationContext;
            _mongoDbContext = mongoDbContext;
            _mapper = mapper;
            _fileService=fileService;
            _paginationService = paginationService;
            _localizer = localizer;
            _entityQueryService = entityQueryService;
        }

        [HttpGet("{id:int}")]
		[Permission(PermissionResource.Publication, CrudActions.View)]
		public async Task<IActionResult> Get(int id)
        {
            var publication = await _applicationContext.Publications.Include(p => p.Keywords)
                                                                    .Include(p => p.Authors)
                                                                    .Include(p => p.Subjects)
                                                                    .Include(p => p.PublicationType)
                                                                    .Include(p => p.Language)
                                                                    .FirstOrDefaultAsync(p => p.Id == id);

			if (publication == null)
                return NotFound(_localizer["ModelNotFound", nameof(Publication), id.ToString()].Value);

            return Ok(publication);
        }

        [HttpGet("{id:int}/file")]
		[Permission(PermissionResource.Publication, CrudActions.View)]
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

        [HttpGet("page")]
		[Permission(PermissionResource.Publication, CrudActions.View)]
		public async Task<IActionResult> GetPagedPublications([FromQuery] int skip = 0, int take = 10)
        {
            var publications = await _paginationService.GetPagedListAsync(_applicationContext.Publications, skip, take);

            return Ok(publications);
        }

        [HttpPost]
		[Permission(PermissionResource.Publication, CrudActions.Create)]
		[LogAction(nameof(Publication), nameof(Create))]
		public async Task<IActionResult> Create([FromForm] Publication publication, PublicationUpdateDto publicationUpdateDto)
        {
			publication.Subjects = await _entityQueryService.GetByIdsAsync(_applicationContext.Subjects, publicationUpdateDto.Subjects);

			publication.Authors = await _entityQueryService.GetByIdsAsync(_applicationContext.Authors, publicationUpdateDto.Authors);

			publication.Keywords = await _entityQueryService.GetByIdsAsync(_applicationContext.Keywords, publicationUpdateDto.Keywords);

			publication.CreatedDate = DateTime.UtcNow;

			publication.LastModifiedDate = DateTime.UtcNow;

			if (publicationUpdateDto.File != null)
            {
                var result = await _fileService.SaveFileAsync(publicationUpdateDto.File, _mongoDbContext.PublicationFiles, MediaTypeNames.Application.Pdf);

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

        [HttpPatch("{id:int}")]
		[Permission(PermissionResource.Publication, CrudActions.Update)]
		[LogAction(nameof(Publication), nameof(Update))]
		public async Task<IActionResult> Update([FromForm] Publication publication, PublicationUpdateDto publicationUpdateDto, int id)
        {
            var existedPublication = await _applicationContext.Publications.FindAsync(id);

			if (existedPublication == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(Publication), id.ToString()].Value);
            }

			existedPublication.Subjects = await _entityQueryService.GetByIdsAsync(_applicationContext.Subjects, publicationUpdateDto.Subjects) ?? existedPublication.Subjects;

			existedPublication.Authors = await _entityQueryService.GetByIdsAsync(_applicationContext.Authors, publicationUpdateDto.Authors) ?? existedPublication.Authors;

			existedPublication.Keywords = await _entityQueryService.GetByIdsAsync(_applicationContext.Keywords, publicationUpdateDto.Keywords) ?? existedPublication.Keywords;

			_mapper.Map(publication, existedPublication);

			if (publicationUpdateDto.File != null)
            {
                var result = await _fileService.UpdateFileAsync(publicationUpdateDto.File, publication.FileId, _mongoDbContext.PublicationFiles, MediaTypeNames.Application.Pdf);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Error?.Message);
                }

				existedPublication.FileId = result.Value!.Id.ToString();
            }

			existedPublication.LastModifiedDate = DateTime.UtcNow;

			await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
		[Permission(PermissionResource.Publication, CrudActions.Delete)]
		[LogAction(nameof(Publication), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var publication = await _applicationContext.Publications.FindAsync(id);

            if (publication == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(Publication), id.ToString()].Value);
            }

            await _fileService.DeleteFileAsync(publication.FileId, _mongoDbContext.PublicationFiles);

            _applicationContext.Publications.Remove(publication);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}