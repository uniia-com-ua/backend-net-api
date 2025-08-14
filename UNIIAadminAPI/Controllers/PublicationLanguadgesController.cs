using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Constants;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/publication-languages")]
    public class PublicationLanguageController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IPaginationService _paginationService;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public PublicationLanguageController(
            ApplicationContext applicationContext,
            IPaginationService paginationService,
			IStringLocalizer<ErrorMessages> localizer)
		{
			_applicationContext = applicationContext;
			_paginationService = paginationService;
            _localizer = localizer;
		}

		[HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var language = await _applicationContext.PublicationLanguages.FirstOrDefaultAsync(l => l.Id == id);

            if (language == null)
                return NotFound(_localizer["ModelNotFound", nameof(PublicationLanguage), id.ToString()].Value);

            return Ok(language);
        }

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPaginated(int skip = 0, int take = 10)
        {
            var languages = await _paginationService.GetPagedListAsync(_applicationContext.PublicationLanguages, skip, take);

            return Ok(languages);
        }

        [HttpPost]
		[LogAction(nameof(PublicationLanguage), nameof(Create))]
		public async Task<IActionResult> Create([FromBody] string name)
        {
            if (!ModelState.IsValid)
                return BadRequest(_localizer["ModelNotValid"].Value);

            PublicationLanguage language = new()
            {
                Name = name
            };

            await _applicationContext.PublicationLanguages.AddAsync(language);

            await _applicationContext.SaveChangesAsync();

			HttpContext.Items.Add("id", language.Id);

			return Ok();
        }

        [HttpPatch]
        [Route("{id}")]
		[LogAction(nameof(PublicationLanguage), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] string name, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(_localizer["ModelNotValid"].Value);

            var language = await _applicationContext.PublicationLanguages.FirstOrDefaultAsync(l => l.Id == id);

            if (language == null)
                return NotFound(_localizer["ModelNotFound", nameof(PublicationLanguage), id.ToString()].Value);

            language.Name = name;

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
		[LogAction(nameof(PublicationLanguage), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var language = await _applicationContext.PublicationLanguages.FirstOrDefaultAsync(l => l.Id == id);

            if (language == null)
                return NotFound(_localizer["ModelNotFound", nameof(PublicationLanguage), id.ToString()].Value);

            _applicationContext.Remove(language);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}
