using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Constants;
using UniiaAdmin.WebApi.Helpers;
using UNIIAadminAPI.Services;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("keywords")]
    public class KeywordController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly ILogActionService _logActionService;

        public KeywordController(
            ApplicationContext applicationContext,
            ILogActionService logActionService)
        {
            _applicationContext = applicationContext;
            _logActionService = logActionService;
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
        [ValidateToken]
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

            await _logActionService.LogActionAsync<Keyword>(HttpContext.Items["User"] as AdminUser, keyword.Id, CrudOperation.Create.ToString());

            return Ok();
        }

        [HttpPatch]
        [Route("{id}")]
        [ValidateToken]
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

            await _logActionService.LogActionAsync<Keyword>(HttpContext.Items["User"] as AdminUser, id, CrudOperation.Update.ToString());

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        [ValidateToken]
        public async Task<IActionResult> Delete(int id)
        {
            var keyword = await _applicationContext.Keywords.FirstOrDefaultAsync(k => k.Id == id);

            if (keyword == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Keyword), id.ToString()));

            _applicationContext.Remove(keyword);

            await _applicationContext.SaveChangesAsync();

            await _logActionService.LogActionAsync<Keyword>(HttpContext.Items["User"] as AdminUser, id, CrudOperation.Delete.ToString());

            return Ok();
        }
    }
}
