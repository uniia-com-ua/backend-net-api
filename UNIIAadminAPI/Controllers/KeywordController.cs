using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
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
    [Route("api/v1/keywords")]
    public class KeywordController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IPaginationService _paginationService;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public KeywordController(
            ApplicationContext applicationContext,
            IPaginationService paginationService)
            IPaginationService paginationService,
            IStringLocalizer<ErrorMessages> localizer)
        {
            _applicationContext = applicationContext;
            _paginationService = paginationService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var keyword = await _applicationContext.Keywords.FirstOrDefaultAsync(k => k.Id == id);

            if (keyword == null)
                return NotFound(_localizer["ModelNotFound", nameof(Keyword), id.ToString()].Value);

            return Ok(keyword);
        }

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPaginatedKeywords(int skip = 0, int take = 10)
        {
            var pagedKeywords = await _paginationService.GetPagedListAsync(_applicationContext.Keywords, skip, take);

            return Ok(pagedKeywords);
        }

        [HttpPost]
		[LogAction(nameof(Keyword), nameof(Create))]
		public async Task<IActionResult> Create([FromBody] string word)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(_localizer["ModelNotValid"].Value);
            }

            Keyword keyword = new()
            {
                Word = word
            };

            await _applicationContext.Keywords.AddAsync(keyword);

            await _applicationContext.SaveChangesAsync();

			HttpContext.Items.Add("id", keyword.Id);

			return Ok();
        }

        [HttpPatch("{id:int}")]
		[LogAction(nameof(Keyword), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] string word, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(_localizer["ModelNotValid"].Value);
            }

            var keyword = await _applicationContext.Keywords.FirstOrDefaultAsync(k => k.Id == id);

            if (keyword == null)
                return NotFound(_localizer["ModelNotFound", nameof(Keyword), id.ToString()].Value);

            keyword.Word = word;

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
		[LogAction(nameof(Keyword), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var keyword = await _applicationContext.Keywords.FirstOrDefaultAsync(k => k.Id == id);

            if (keyword == null)
                return NotFound(_localizer["ModelNotFound", nameof(Keyword), id.ToString()].Value);

            _applicationContext.Remove(keyword);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}
