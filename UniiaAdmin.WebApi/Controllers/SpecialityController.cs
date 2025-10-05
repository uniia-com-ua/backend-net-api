using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net.Mime;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UniiaAdmin.WebApi.Controllers
{
	[ApiController]
    [Route("api/v1/specialities")]
    public class SpecialityController : ControllerBase
    {
		private readonly IApplicationUnitOfWork _applicationUnitOfWork;
		private readonly IGenericRepository _genericRepository;
		private readonly IMapper _mapper;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public SpecialityController(IStringLocalizer<ErrorMessages> localizer,
			IMapper mapper,
			IApplicationUnitOfWork applicationUnitOfWork,
			IGenericRepository genericRepository)
		{
			_localizer = localizer;
			_applicationUnitOfWork = applicationUnitOfWork;
			_genericRepository = genericRepository;
			_mapper = mapper;
		}

		[Permission(PermissionResource.Speciality, CrudActions.View)]
		[HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var speciality = await _applicationUnitOfWork.GetByIdWithIncludesAsync<Specialty>(x => x.Id == id, x => x.Subjects!);

			if (speciality == null)
                return NotFound(_localizer["ModelNotFound", nameof(Specialty), id.ToString()].Value);

            return Ok(_mapper.Map<SpecialityDto>(speciality));
        }

		[Permission(PermissionResource.Speciality, CrudActions.View)]
		[HttpGet("page")]
        public async Task<IActionResult> GetPaged([FromQuery] int skip = 0, int take = 10)
        {
			var pagedSpecialties = await _applicationUnitOfWork.GetPagedWithIncludesAsync<Specialty>(skip, take, x => x.Subjects!);

			var pagedSpecialtiesDto = new PageData<SpecialityDto>()
			{
				Items = pagedSpecialties.Items.Select(x => _mapper.Map<SpecialityDto>(x)).ToList(),
				TotalCount = pagedSpecialties.TotalCount
			};

			return Ok(pagedSpecialtiesDto);
        }

		[Permission(PermissionResource.Speciality, CrudActions.Create)]
		[HttpPost]
		[LogAction(nameof(Specialty), nameof(Create))]
		public async Task<IActionResult> Create([FromBody] Specialty specialty)
        {
			await _genericRepository.CreateAsync(specialty);

			HttpContext.Items.Add("id", specialty.Id);

            return Ok();
        }

		[Permission(PermissionResource.Speciality, CrudActions.Update)]
		[HttpPatch("{id:int}")]
		[LogAction(nameof(Specialty), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] Specialty specialty, int id)
        {
            var existedSpecialty = await _applicationUnitOfWork.FindAsync<Specialty>(id);

			if (existedSpecialty == null)
            {
                return NotFound(_localizer["ModelNotFound", nameof(Specialty), id.ToString()].Value);
            }

			await _genericRepository.UpdateAsync(specialty, existedSpecialty);

            return Ok();
        }

		[Permission(PermissionResource.Speciality, CrudActions.Delete)]
		[HttpDelete("{id:int}")]
		[LogAction(nameof(Specialty), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
			var specialty = await _applicationUnitOfWork.FindAsync<Specialty>(id);

			if (specialty == null)
                return NotFound(_localizer["ModelNotFound", nameof(Specialty), id.ToString()].Value);

            await _genericRepository.DeleteAsync(specialty);

            return Ok();
        }
    }
}