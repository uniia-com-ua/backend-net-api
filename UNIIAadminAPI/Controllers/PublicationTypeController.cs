using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Enums;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Constants;
using UniiaAdmin.WebApi.Services;
using UNIIAadminAPI.Services;

namespace UniiaAdmin.WebApi.Controllers
{
    [ApiController]
    [Route("publication-types")]
    public class PublicationTypeController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly LogActionService _logActionService;

        public PublicationTypeController(
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
            var publicationType = await _applicationContext.PublicationTypes.FirstOrDefaultAsync(pt => pt.Id == id);

            if (publicationType == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(PublicationType), id.ToString()));

            return Ok(publicationType);
        }

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPaginated(int skip, int take)
        {
            var publicationTypes = await PaginationHelper.GetPagedListAsync(_applicationContext.PublicationTypes, skip, take);

            return Ok(publicationTypes);
        }

        [HttpPost]
        [Route("create")]
        [ValidateToken]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorMessages.ModelNotValid);

            PublicationType publicationType = new()
            {
                Name = name
            };

            await _applicationContext.PublicationTypes.AddAsync(publicationType);

            await _applicationContext.SaveChangesAsync();

            await _logActionService.LogActionAsync<PublicationType>(HttpContext.Items["User"] as AdminUser, publicationType.Id, CrudOperation.Create.ToString());

            return Ok();
        }

        [HttpPut]
        [Route("{id}/update")]
        [ValidateToken]
        public async Task<IActionResult> Update([FromBody] string name, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorMessages.ModelNotValid);

            var publicationType = await _applicationContext.PublicationTypes.FirstOrDefaultAsync(pt => pt.Id == id);

            if (publicationType == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(PublicationType), id.ToString()));

            publicationType.Name = name;

            await _applicationContext.SaveChangesAsync();

            await _logActionService.LogActionAsync<PublicationType>(HttpContext.Items["User"] as AdminUser, id, CrudOperation.Update.ToString());

            return Ok();
        }

        [HttpDelete]
        [Route("{id}/delete")]
        [ValidateToken]
        public async Task<IActionResult> Delete(int id)
        {
            var publicationType = await _applicationContext.PublicationTypes.FirstOrDefaultAsync(pt => pt.Id == id);

            if (publicationType == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(PublicationType), id.ToString()));

            _applicationContext.Remove(publicationType);

            await _applicationContext.SaveChangesAsync();

            await _logActionService.LogActionAsync<PublicationType>(HttpContext.Items["User"] as AdminUser, id, CrudOperation.Delete.ToString());

            return Ok();
        }
    }
}
