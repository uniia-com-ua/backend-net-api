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

namespace UniiaAdmin.WebApi.Controllers
{
	[Authorize]
	[ApiController]
    [Route("subjects")]
    public class SubjectController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;

        public SubjectController(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
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
        [LogAction(nameof(Subject), nameof(Create))]
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

			HttpContext.Items.Add("id", subject.Id);

			return Ok();
        }

        [HttpPatch]
        [Route("{id}")]
		[LogAction(nameof(Subject), nameof(Update))]
		public async Task<IActionResult> Update([FromBody] string name, int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ErrorMessages.ModelNotValid);

            var subject = await _applicationContext.Subjects.FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Subject), id.ToString()));

            subject.Name = name;

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
		[LogAction(nameof(Subject), nameof(Delete))]
		public async Task<IActionResult> Delete(int id)
        {
            var subject = await _applicationContext.Subjects.FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null)
                return NotFound(ErrorMessages.ModelNotFound(nameof(Subject), id.ToString()));

            _applicationContext.Remove(subject);

            await _applicationContext.SaveChangesAsync();

            return Ok();
        }
    }
}
