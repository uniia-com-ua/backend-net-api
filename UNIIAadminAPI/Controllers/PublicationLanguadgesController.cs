﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Constants;
using UniiaAdmin.WebApi.Helpers;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("publication-languages")]
    public class PublicationLanguageController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;

        public PublicationLanguageController(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var language = await _applicationContext.PublicationLanguages.FirstOrDefaultAsync(l => l.Id == id);

            if (language == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(PublicationLanguage), id.ToString()));

            return Ok(language);
        }

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPaginated(int skip, int take)
        {
            var languages = await PaginationHelper.GetPagedListAsync(_applicationContext.PublicationLanguages, skip, take);

            return Ok(languages);
        }

        [HttpPost]
		[LogAction(nameof(PublicationLanguage), nameof(Create))]
		public async Task<IActionResult> Create([FromBody] string name)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorMessages.ModelNotValid);

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
                return BadRequest(ErrorMessages.ModelNotValid);

            var language = await _applicationContext.PublicationLanguages.FirstOrDefaultAsync(l => l.Id == id);

            if (language == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(PublicationLanguage), id.ToString()));

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
                return NotFound(ErrorMessages.ModelNotFound(nameof(PublicationLanguage), id.ToString()));

            _applicationContext.Remove(language);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}
