using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Auth.Controllers
{
	[ApiController]
	[Route("admin/api/auth")]
	public class AuthController : ControllerBase
	{
		private readonly IJwtAuthenticator _jwtAuthenticator;

		public AuthController(IJwtAuthenticator jwtAuthenticator)
		{
			_jwtAuthenticator = jwtAuthenticator;
		}

		[HttpGet]
		[Route("signingoogle")]
		public IActionResult Login() => Challenge(GoogleDefaults.AuthenticationScheme);

		[HttpGet]
		[Route("tokens")]
		[ProducesResponseType(typeof(UserTokens), StatusCodes.Status200OK)]
		public IActionResult Tokens([FromQuery] string accessToken, string refreshToken)
		{
			return Ok(new UserTokens
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken
			});
		}

		[HttpGet]
		[Route("refresh")]
		public async Task<IActionResult> RefreshToken(string accessToken, string? refreshToken)
		{
			var principals = _jwtAuthenticator.GetPrincipalFromExpiredToken(accessToken);

			if (principals == null)
			{
				return BadRequest("Access token is not valid");
			}

			var email = principals.FindFirstValue(ClaimTypes.Email);

			if (string.IsNullOrEmpty(email))
			{
				return BadRequest("Access token is not valid");
			}

			if(!await _jwtAuthenticator.IsRefreshTokenValidAsync(email, refreshToken))
			{
				return BadRequest("Refresh token was expired");
			}

			var newToken = _jwtAuthenticator.GenerateAccessToken(principals.Claims);

			return Ok(newToken);
		}
	}
}
