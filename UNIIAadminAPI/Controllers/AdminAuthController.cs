using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
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
    [Route("auth")]
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


        [HttpGet]
        [Route("get-auth-user")]
        [ProducesResponseType(typeof(AdminUser), 200)]
        public async Task<IActionResult> GetAuthUser()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

			var mappedUser = _mapper.Map<UserDto>(user);

            mappedUser.Roles.AddRange(HttpContext.User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value));

            return Ok(mappedUser);
        }

		[HttpGet]
        [Route("get-auth-user-picture")]
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
    }
}
