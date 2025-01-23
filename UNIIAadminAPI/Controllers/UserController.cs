using Amazon.Runtime.Internal;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbGenericRepository;
using System.Security.Claims;
using UNIIAadminAPI.Enums;
using UNIIAadminAPI.Models;
using UNIIAadminAPI.Serializers;
using UNIIAadminAPI.Services;

namespace UNIIAadminAPI.Controllers
{
    [Route("admin/api/user")]
    [ApiController]
    public class UserController(UserManager<MongoIdentityUser> userManager, RoleManager<MongoIdentityRole> roleManager, IMongoDbContext db) : ControllerBase
    {
        [HttpPatch]
        [Route("add-claim-to-user")]
        [ValidateToken]
        [Authorize(Roles = "Admin")]
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

            if(!result.Succeeded)
                return BadRequest();

            return Ok();
        }

        [HttpPatch]
        [Route("remove-claim-from-user")]
        [ValidateToken]
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
        }

/*        [HttpGet]
        [Route("getAll")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewAdminUsers)]
        public async Task<List<UserDto>> GetAllAdminUsers()
        {
            List<UserDto> userDtos = [];

            adminUserManager.Users;

            var users = db.GetCollection<User>().AsQueryable();

            var roles = roleManager.Roles;

            foreach (var user in users)
            {
                var faculty = await db.GetCollection<Faculty>("Faculties")
                .Find(f => f.Id == user.FacultyId)
                .FirstOrDefaultAsync();

                var userRoles = roles.Where(r => user.Roles.Contains(r.Id)).Select(r => r.Name);

                userDtos.Add(new(user, userRoles, faculty));
            }

            return userDtos;
        }*/

        [HttpGet]
        [Route("get-all-users")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewUsers)]
        public async Task<List<UserDto>> GetAllUsers()
        {
            List<UserDto> userDtos = [];

            var users = db.GetCollection<User>().AsQueryable();

            var relatedUserCollection = db.GetCollection<MongoIdentityUser>().AsQueryable();

            var roles = roleManager.Roles;

            foreach (var user in users)
            {
                var faculty = await db.GetCollection<Faculty>("Faculties")
                .Find(f => f.Id == user.FacultyId)
                .FirstOrDefaultAsync();

                var relatedUser = relatedUserCollection.FirstOrDefault(u => u.Id == user.AuthUserId);

                var userRoles = roles.Where(r => relatedUser.Roles.Contains(r.Id)).Select(r => r.Name);

                userDtos.Add(new UserDto(user, relatedUser, userRoles, faculty));
            }

            return userDtos;
        }

        [HttpGet]
        [Route("get-by-id")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewUsers)]
        public async Task<IActionResult> GetUserById(string id)
        {
            var relatedUser = await userManager.FindByIdAsync(id);

            if (relatedUser == null)
                return NotFound();

            var user = await db.GetCollection<User>().Find(u => u.AuthUserId == relatedUser.Id).FirstOrDefaultAsync();

            var userRoles = roleManager.Roles.Where(r => relatedUser.Roles.Contains(r.Id)).Select(r => r.Name);
            
            var faculty = await db.GetCollection<Faculty>("Faculties")
            .Find(f => f.Id == user.FacultyId)
            .FirstOrDefaultAsync();

            return Ok(new UserDto(user, relatedUser, userRoles, faculty));
        }

        [HttpGet]
        [Route("get-log-history")]
        [ValidateToken]
        public async Task<IActionResult> GetLogHistory()
        {
            var relatedUser = await userManager.GetUserAsync(HttpContext.User);

            var user = await db.GetCollection<AdminUser>().Find(u => u.AuthUserId == relatedUser.Id).FirstOrDefaultAsync();

            var logInEnumerable = db.GetCollection<LogInHistory>().Find(log => log.UserId == user.Id).ToEnumerable().TakeLast(20);

            List<LogInHistoryDto> logInHistoryDtos = [];

            foreach(var log in logInEnumerable)
            {
                logInHistoryDtos.Add(new LogInHistoryDto(log));
            }

            return Ok(logInHistoryDtos);
        }
    }
}
