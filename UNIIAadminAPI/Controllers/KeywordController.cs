using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
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
    [Route("keywords")]
    public class KeywordController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;

        public KeywordController(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var keyword = await _applicationContext.Keywords.FirstOrDefaultAsync(k => k.Id == id);

            if (keyword == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Keyword), id.ToString()));

            return Ok(keyword);
        }

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPaginatedKeywords(int skip, int take)
        {
            var pagedKeywords = await PaginationHelper.GetPagedListAsync(_applicationContext.Keywords, skip, take);

            return Ok(pagedKeywords);
        }

        [HttpPost]
		[LogAction(nameof(Keyword), nameof(Create))]
		public async Task<IActionResult> Create([FromBody] string word)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorMessages.ModelNotValid);
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

        [HttpPatch]
        [Route("{id}")]
		[LogAction(nameof(Keyword), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] string word, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorMessages.ModelNotValid);
            }

            var keyword = await _applicationContext.Keywords.FirstOrDefaultAsync(k => k.Id == id);

            if (keyword == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Keyword), id.ToString()));

            keyword.Word = word;

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
		[LogAction(nameof(Keyword), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var keyword = await _applicationContext.Keywords.FirstOrDefaultAsync(k => k.Id == id);

            if (keyword == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Keyword), id.ToString()));

            _applicationContext.Remove(keyword);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}
