using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDbGenericRepository;
using System.Security.Claims;
using UNIIAadminAPI.Models;
using UNIIAadminAPI.Serializers;
using UNIIAadminAPI.Services;


namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("admin/api/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly UserManager<MongoIdentityUser> _userManager;
        private readonly SignInManager<MongoIdentityUser> _signInManager;
        private readonly IMongoDbContext _db;
        private readonly TokenService _tokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        public AdminAuthController
            (UserManager<MongoIdentityUser> userManager, 
            SignInManager<MongoIdentityUser> signInManager,  
            IMongoDbContext db, 
            TokenService tokenService, 
            IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _tokenService = tokenService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [Route("signingoogle")]
        public IActionResult Login()
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", "AdminAuth", null, Request.Scheme),
            };

            return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [Route("googlecallback")]
        public async Task<IActionResult> GoogleCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return BadRequest();

            var accessToken = authenticateResult.Ticket.Properties.GetTokenValue("access_token")!;
            var refreshToken = authenticateResult.Ticket.Properties.GetTokenValue("refresh_token")!;

            var claims = authenticateResult.Principal.Claims;

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value;

            MongoIdentityUser? user = await _userManager.FindByEmailAsync(email);

            AdminUser adminUser;

            if (user == null)
            {
                var profilePictureTask = ClaimUserService.GetUserPictureFromClaims(claims, _httpClientFactory.CreateClient());

                user = new()
                {
                    Email = email,
                    UserName = email,
                    Tokens = _tokenService.MakeEncryptedTokenList(accessToken, refreshToken)
                };

                adminUser = new()
                {
                    ProfilePicture = await profilePictureTask,
                    AuthUserId = user.Id,
                    LastSingIn = DateTime.UtcNow,
                    IsOnline = true,
                    Name = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)!.Value,
                    Surname = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)!.Value,
                };

                await _db.GetCollection<AdminUser>().InsertOneAsync(adminUser);

                await _userManager.CreateAsync(user);

                await _userManager.AddToRoleAsync(user, "EMPTYADMIN");
            }
            else
            {
                adminUser = await _db.GetCollection<AdminUser>()
                                     .Find(u => u.AuthUserId == user.Id)
                                     .FirstOrDefaultAsync();
                
                user.Tokens = _tokenService.MakeEncryptedTokenList(accessToken, refreshToken);

                await _userManager.UpdateAsync(user);

                var filter = Builders<AdminUser>.Filter.Eq(u => u.Id, adminUser.Id);
                
                var update = Builders<AdminUser>.Update.Set(u => u.LastSingIn, adminUser.LastSingIn);
                
                await _db.GetCollection<AdminUser>().UpdateOneAsync(filter, update);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            AuthService authService = new(HttpContext);

            await authService.AddLoginInfoToHistory(adminUser);

            return Redirect("/admin");
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
    
            return Ok();
        }

        [HttpGet]
        [Route("get-auth-user")]
        [ValidateToken]
        public async Task<IActionResult> GetAuthUser()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            if (user == null)
            {
                return Unauthorized();
            }

            var adminUser = await _db.GetCollection<AdminUser>().Find(u => u.AuthUserId == user.Id).FirstOrDefaultAsync();

            return Ok(new AdminUserDto(adminUser, user));
        }

        [HttpGet]
        [Route("get-auth-user-picture")]
        [ValidateToken]
        public async Task<IActionResult> GetAuthUserPicture()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var adminUser = await _db.GetCollection<AdminUser>()
                                    .Find(u => u.AuthUserId == user!.Id)
                                    .FirstOrDefaultAsync();

            if(adminUser?.ProfilePicture == null)
            {
                return NotFound();
            }

            var contentType = "image/jpeg";

            return File(adminUser.ProfilePicture, contentType);
        }
    }
}
