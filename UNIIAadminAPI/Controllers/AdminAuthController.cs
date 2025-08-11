using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Models;


namespace UNIIAadminAPI.Controllers
{
    [Authorize]
	[ApiController]
	[Route("api/v1/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly UserManager<AdminUser> _userManager;
		private readonly MongoDbContext _mongoDbContext;
        private readonly IMapper _mapper;
        public AdminAuthController
            (UserManager<AdminUser> userManager, 
            MongoDbContext mongoDbContext,
            IMapper mapper)
        {
            _userManager = userManager;
            _mongoDbContext = mongoDbContext;
            _mapper = mapper;
        }


        [HttpGet("user")]
        [ProducesResponseType(typeof(AdminUser), 200)]
        public async Task<IActionResult> GetAuthUser()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

			var mappedUser = _mapper.Map<AdminUserDto>(user);

            mappedUser.Roles.AddRange(HttpContext.User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value));

            return Ok(mappedUser);
        }

		[HttpGet]
        [Route("picture")]
        [ProducesResponseType(typeof(byte[]), 200)]
		public async Task<IActionResult> GetAuthUserPicture()
        {
			var user = await _userManager.GetUserAsync(HttpContext.User);

			var photoId = ObjectId.Parse(user!.ProfilePictureId);

            var photoFile = await _mongoDbContext.UserPhotos.FirstOrDefaultAsync(ph => ph.Id == photoId);

            if(photoFile == null || photoFile.File == null)
            {
                return NotFound();
            }

            var contentType = "image/jpeg";

            return File(photoFile.File, contentType);
        }

		[HttpGet("log-history")]
		public async Task<IActionResult> GetLogHistory(int skip = 0, int take = 10)
		{
			var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

			var logInHistory = await _mongoDbContext.AdminLogInHistories
				.Where(li => li.UserId == userId)
				.OrderByDescending(li => li.LogInTime)
				.Skip(skip)
				.Take(take)
				.Select(li => new
				{
					li.IpAdress,
					li.LogInType,
					li.LogInTime,
					li.UserAgent
				})
				.ToListAsync();

			return Ok(logInHistory);
		}
	}
}
