using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Constants;
using UniiaAdmin.WebApi.Services;
using UNIIAadminAPI.Services;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("publication-languages")]
    public class PublicationLanguageController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly LogActionService _logActionService;

        public PublicationLanguageController(
            ApplicationContext applicationContext,
            LogActionService logActionService)
        {
            _applicationContext = applicationContext;
            _logActionService = logActionService;
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
        [ValidateToken]
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

            await _logActionService.LogActionAsync<PublicationLanguage>(HttpContext.Items["User"] as AdminUser, language.Id, CrudOperation.Create.ToString());

            return Ok();
        }

        [HttpPatch]
        [Route("{id}")]
        [ValidateToken]
        public async Task<IActionResult> Update([FromBody] string name, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorMessages.ModelNotValid);

            var language = await _applicationContext.PublicationLanguages.FirstOrDefaultAsync(l => l.Id == id);

            if (language == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(PublicationLanguage), id.ToString()));

            language.Name = name;

            await _applicationContext.SaveChangesAsync();

            await _logActionService.LogActionAsync<PublicationLanguage>(HttpContext.Items["User"] as AdminUser, id, CrudOperation.Update.ToString());
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        [ValidateToken]
        public async Task<IActionResult> Delete(int id)
        {
            var language = await _applicationContext.PublicationLanguages.FirstOrDefaultAsync(l => l.Id == id);

            if (language == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(PublicationLanguage), id.ToString()));

            _applicationContext.Remove(language);

            await _applicationContext.SaveChangesAsync();

            await _logActionService.LogActionAsync<PublicationLanguage>(HttpContext.Items["User"] as AdminUser, id, CrudOperation.Delete.ToString());
            return Ok();
        }
    }
}
