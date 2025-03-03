using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
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
using UNIIAadminAPI.Services;


namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("admin/api/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly UserManager<AdminUser> _userManager;
        private readonly SignInManager<AdminUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly MongoDbContext _mongoDbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        public AdminAuthController
            (UserManager<AdminUser> userManager, 
            SignInManager<AdminUser> signInManager,
            MongoDbContext mongoDbContext,
            ITokenService tokenService, 
            IHttpClientFactory httpClientFactory,
            IMapper mapper,
            IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mongoDbContext = mongoDbContext;
            _tokenService = tokenService;
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _authService = authService;
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
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return BadRequest();

            var accessToken = authenticateResult.Ticket.Properties.GetTokenValue("access_token")!;

            var refreshToken = authenticateResult.Ticket.Properties.GetTokenValue("refresh_token")!;

            var claims = authenticateResult.Principal.Claims.ToList();

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)!.Value;

            AdminUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                using var httpClient = _httpClientFactory.CreateClient();

                var profilePictureTask = ClaimUserService.GetUserPictureFromClaims(claims, httpClient);

                user = new AdminUser()
                {
                    Email = email,
                    LastSingIn = DateTime.UtcNow,
                    UserName = email,
                    IsOnline = true,
                    Name = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)!.Value,
                    Surname = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)!.Value,
                };

                AdminUserPhoto photo = new()
                {
                    Id = ObjectId.GenerateNewId(),
                    File = await profilePictureTask
                };

                user.ProfilePictureId = photo.Id.ToString();

                await _mongoDbContext.UserPhotos.AddAsync(photo);

                await _mongoDbContext.SaveChangesAsync();

                await _userManager.CreateAsync(user);
            }

            await _userManager.SetAuthenticationTokenAsync(user, "Uniia", "accessToken", _tokenService.EncryptToken(accessToken));

            await _userManager.SetAuthenticationTokenAsync(user, "Uniia", "refreshToken", _tokenService.EncryptToken(refreshToken));

            await _signInManager.SignInAsync(user, isPersistent: false);

            await _authService.AddLoginInfoToHistory(user, HttpContext);

            return Ok();
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
        [ProducesResponseType(typeof(AdminUser), 200)]
        [ValidateToken]
        public async Task<IActionResult> GetAuthUser()
        {
            var user = HttpContext.Items["User"] as AdminUser;

            if (user == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);

            var mappedUser = _mapper.Map<UserDto>(user);

            mappedUser.Roles = roles;

            return Ok(mappedUser);
        }

        [HttpGet]
        [Route("get-auth-user-picture")]
        [ValidateToken]
        public async Task<IActionResult> GetAuthUserPicture()
        {
            var user = HttpContext.Items["User"] as AdminUser;

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
