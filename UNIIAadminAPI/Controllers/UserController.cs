using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

namespace UNIIAadminAPI.Controllers
{
    [Route("admin/api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AdminUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationContext _applicationContext;
        private readonly MongoDbContext _mongoDbContext;
        public UserController(
            UserManager<AdminUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationContext applicationContext,
            MongoDbContext mongoDbContext) 
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mongoDbContext = mongoDbContext;
            _applicationContext = applicationContext;
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
        [Route("get-all-users")]
        
        public async Task<IActionResult> GetAllUsers(int skip, int take)
        {
            var users = await _applicationContext.Users.Skip(skip).Take(take).ToListAsync();

            return Ok(users);
        }

/*        [HttpGet]
        [Route("get-by-id")]
        
        public async Task<IActionResult> GetUserById(string id)
        {
            var relatedUser = await _userManager.FindByIdAsync(id);

            if (relatedUser == null)
                return NotFound();

            var user = await db.GetCollection<User>().Find(u => u.AuthUserId == relatedUser.Id).FirstOrDefaultAsync();

            var userRoles = roleManager.Roles.Where(r => relatedUser.Roles.Contains(r.Id)).Select(r => r.Name);

            var faculty = await db.GetCollection<Faculty>("Faculties")
            .Find(f => f.Id == user.FacultyId)
            .FirstOrDefaultAsync();

            return Ok(new UserDto(user, relatedUser, userRoles, faculty));
        }*/

        [HttpGet]
        [Route("get-log-history")]
        
        public async Task<IActionResult> GetLogHistory(int take)
        {
            var user = HttpContext.Items["User"] as AdminUser;

            var logInHistory = await _mongoDbContext.AdminLogInHistories
                .Where(li => li.UserId == user!.Id)
                .OrderByDescending(li => li.LogInTime)
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
