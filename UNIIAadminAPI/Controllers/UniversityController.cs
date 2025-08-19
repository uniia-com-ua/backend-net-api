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
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("api/v1/universities")]
    public class UniversityController : ControllerBase
    {
		private readonly ISmallPhotoRepository _smallPhotoRepository;
		private readonly IPhotoProvider _photoProvider;
		private readonly IApplicationUnitOfWork _applicationUnitOfWork;
		private readonly IQueryRepository _queryRepository;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public UniversityController(
			ISmallPhotoRepository smallPhotoRepository,
			IStringLocalizer<ErrorMessages> localizer,
            IApplicationUnitOfWork applicationUnitOfWork,
			IPhotoProvider photoProvider,
			IQueryRepository queryRepository)
		{
			_localizer = localizer;
            _smallPhotoRepository = smallPhotoRepository;
			_photoProvider = photoProvider;
			_queryRepository = queryRepository;
            _applicationUnitOfWork = applicationUnitOfWork;
		}

		[HttpGet("{id:int}")]
		[Permission(PermissionResource.University, CrudActions.View)]
		public async Task<IActionResult> Get(int id)
        {
            var university = await _applicationUnitOfWork.FindAsync<University>(id);

            if (university == null)
                return NotFound(_localizer["ModelNotFound", nameof(University), id.ToString()].Value);

            return Ok(university);
        }

        [HttpGet("{id:int}/photo")]
		[Permission(PermissionResource.University, CrudActions.View)]
		public async Task<IActionResult> GetPicture(int id)
        {
            var result = await _photoProvider.GetPhotoAsync<University, UniversityPhoto>(id);

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
			var result = await _smallPhotoRepository.GetSmallPhotoAsync<University, UniversityPhoto>(id);

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

        [HttpGet("page")]
		[Permission(PermissionResource.University, CrudActions.View)]
		public async Task<IActionResult> GetPagedUniversities(int skip = 0, int take = 10)
        {
            var universitiesList = await _queryRepository.GetPagedAsync<University>(skip, take);

            return Ok(universitiesList);
        }

        [HttpPost]
		[Permission(PermissionResource.University, CrudActions.Create)]
		[LogAction(nameof(University), nameof(Create))]
        public async Task<IActionResult> Create([FromForm] University university, IFormFile? photoFile, IFormFile? smallPhotoFile)
        {
            var result = await _smallPhotoRepository.CreateAsync<University, UniversityPhoto>(university, photoFile, smallPhotoFile);

			if (!result.IsSuccess)
			{
				return BadRequest(result.Error?.Message);
			}

			HttpContext.Items.Add("id", university.Id);

            return Ok();
        }

        [HttpPatch("{id:int}")]
		[Permission(PermissionResource.University, CrudActions.Update)]
		[LogAction(nameof(University), nameof(Update))]
        public async Task<IActionResult> Update([FromForm] University university, IFormFile? photoFile, IFormFile? smallPhotoFile, int id)
        {
            var existedUniversity = await _applicationUnitOfWork.FindAsync<University>(id);

            if (existedUniversity == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(University), id.ToString()].Value);
            }

            var result = await _smallPhotoRepository.UpdateAsync<University, UniversityPhoto>(university, existedUniversity, photoFile, smallPhotoFile);

			if (!result.IsSuccess)
			{
				return BadRequest(result.Error?.Message);
			}

			return Ok();
        }

        [HttpDelete("{id:int}")]
		[Permission(PermissionResource.University, CrudActions.Delete)]
		[LogAction(nameof(University), nameof(Delete))]
        public async Task<IActionResult> Delete(int id)
        {
            var university = await _applicationUnitOfWork.FindAsync<University>(id);

			if (university == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(University), id.ToString()].Value);
            }

            await _smallPhotoRepository.DeleteAsync<University, UniversityPhoto>(university);

            return Ok();
        }
    }
}
