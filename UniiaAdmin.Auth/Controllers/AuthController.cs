using AutoMapper;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System.Security.Claims;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Models;
using UniiaAdmin.Data.Models.AuthModels;

namespace UniiaAdmin.Auth.Controllers;

[ApiController]
[Route("api/v1")]
public class AuthController : ControllerBase
{
	private readonly IJwtAuthenticator _jwtAuthenticator;
	private readonly UserManager<AdminUser> _userManager;
	private readonly MongoDbContext _mongoDbContext;
	private readonly IMapper _mapper;

	public AuthController(
		IJwtAuthenticator jwtAuthenticator,
		UserManager<AdminUser> userManager,
		MongoDbContext mongoDbContext,
		IMapper mapper)
	{
		_jwtAuthenticator = jwtAuthenticator;
		_userManager = userManager;
		_mongoDbContext = mongoDbContext;
		_mapper = mapper;
	}

	[HttpGet("signingoogle")]
	public IActionResult Login() => Challenge(GoogleDefaults.AuthenticationScheme);

	[HttpGet("tokens")]
	[ProducesResponseType(typeof(UserTokens), StatusCodes.Status200OK)]
	public IActionResult Tokens([FromQuery] UserTokens userTokens)
	{


		return Ok(userTokens);
	}

	[HttpGet("refresh")]
	public async Task<IActionResult> RefreshToken(string accessToken, string? refreshToken)
	{
		var principals = _jwtAuthenticator.GetPrincipalFromExpiredToken(accessToken);

		if (principals == null)
		{
			return BadRequest("Access token is not valid");
		}

		var id = principals.FindFirstValue(ClaimTypes.NameIdentifier);

		if (string.IsNullOrEmpty(id))
		{
			return BadRequest("Access token is not valid");
		}

		if (!await _jwtAuthenticator.IsRefreshTokenValidAsync(id, refreshToken))
		{
			return BadRequest("Refresh token was expired");
		}

		var newToken = _jwtAuthenticator.GenerateAccessToken(principals.Claims);

		return Ok(newToken);
	}

	[Authorize]
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

	[Authorize]
	[HttpGet("picture")]
	[ProducesResponseType(typeof(byte[]), 200)]
	public async Task<IActionResult> GetAuthUserPicture()
	{
		var user = await _userManager.GetUserAsync(HttpContext.User);

		var photoId = ObjectId.Parse(user!.ProfilePictureId);

		var photoFile = await _mongoDbContext.UserPhotos.FirstOrDefaultAsync(ph => ph.Id == photoId);

		if (photoFile == null || photoFile.File == null)
		{
			return NotFound();
		}

		var contentType = "image/jpeg";

		return File(photoFile.File, contentType);
	}

	[Authorize]
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

