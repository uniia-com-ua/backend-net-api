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
    [Route("subjects")]
    public class SubjectController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly LogActionService _logActionService;

        public SubjectController(
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
            var subject = await _applicationContext.Subjects.FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Subject), id.ToString()));

            return Ok(subject);
        }

        [HttpGet]
        [Route("page")]
        public async Task<IActionResult> GetPaginated(int skip, int take)
        {
            var subjects = await PaginationHelper.GetPagedListAsync(_applicationContext.Subjects, skip, take);

            return Ok(subjects);
        }

        [HttpPost]
        [Route("create")]
        [ValidateToken]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorMessages.ModelNotValid);

            Subject subject = new()
            {
                Name = name
            };

            await _applicationContext.Subjects.AddAsync(subject);

            await _applicationContext.SaveChangesAsync();

            await _logActionService.LogActionAsync<Subject>(HttpContext.Items["User"] as AdminUser, subject.Id, CrudOperation.Create.ToString());

            return Ok();
        }

        [HttpPut]
        [Route("{id}/update")]
        [ValidateToken]
        public async Task<IActionResult> Update([FromBody] string name, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorMessages.ModelNotValid);

            var subject = await _applicationContext.Subjects.FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Subject), id.ToString()));

            subject.Name = name;

            await _applicationContext.SaveChangesAsync();

            await _logActionService.LogActionAsync<Subject>(HttpContext.Items["User"] as AdminUser, id, CrudOperation.Update.ToString());

            return Ok();
        }

        [HttpDelete]
        [Route("{id}/delete")]
        [ValidateToken]
        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _applicationContext.Subjects.FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Subject), id.ToString()));

            _applicationContext.Remove(subject);

            await _applicationContext.SaveChangesAsync();

            await _logActionService.LogActionAsync<Subject>(HttpContext.Items["User"] as AdminUser, id, CrudOperation.Delete.ToString());

            return Ok();
        }
    }
}
