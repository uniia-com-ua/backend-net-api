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
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;

namespace UNIIAadminAPI.Controllers
{
    [Authorize]
    [Route("api/v1/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly MongoDbContext _mongoDbContext;
        private readonly IPaginationService _paginationService;
		private readonly IMapper _mapper;
		public UserController(
            ApplicationContext applicationContext,
            MongoDbContext mongoDbContext,
            IPaginationService paginationService,
            IMapper mapper) 
        {
            _mongoDbContext = mongoDbContext;
            _applicationContext = applicationContext;
            _paginationService = paginationService;
            _mapper = mapper;
        }

/*        [HttpPatch]
        [Route("add-claim-to-user")]
        
        public async Task<IActionResult> AddAdminUserClaim(ClaimsEnum claimsEnum, string userId)
        {
            ObjectId objectId = new(userId);

            var authUserId = await ClaimUserService.GetAuthUserIdByUserId(objectId, db);

            if (string.IsNullOrEmpty(authUserId))
            {
                return BadRequest();
            }

            var relatedUser = await userManager.FindByIdAsync(authUserId);

            Claim claim = new("http://schemas.microsoft.com/ws/2008/06/identity/claims/user", claimsEnum.ToString());

            var result = await userManager.AddClaimAsync(relatedUser, claim);

            if (!result.Succeeded)
                return BadRequest();

            return Ok();
        }

        [HttpPatch]
        [Route("remove-claim-from-user")]
        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveAdminUserClaim(ClaimsEnum claimsEnum, string userId)
        {
            ObjectId objectId = new(userId);

            var authUserId = await ClaimUserService.GetAuthUserIdByUserId(objectId, db);

            if (string.IsNullOrEmpty(authUserId))
                return NotFound();

            Claim claim = new("http://schemas.microsoft.com/ws/2008/06/identity/claims/user", claimsEnum.ToString());

            var relatedUser = await userManager.FindByIdAsync(authUserId);

            var result = await userManager.RemoveClaimAsync(relatedUser, claim);

            if (!result.Succeeded)
                return NotFound();

            return Ok();
        }*/

        [HttpGet]
        public async Task<IActionResult> GetAllUsers(int skip, int take = 10)
        {
            var users = await _paginationService.GetPagedListAsync(_applicationContext.Users, skip, take);

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _applicationContext.Users.FindAsync(id);

            if (user == null)
                return NotFound();

			var result = _mapper.Map<UserDto>(user);

            return Ok(result);
        }
    }
}
