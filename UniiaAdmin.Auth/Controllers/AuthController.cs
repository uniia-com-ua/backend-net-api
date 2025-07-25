using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Models.AuthModels;

namespace UniiaAdmin.Auth.Controllers
{
    [ApiController]
	[Route("api/v1")]
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
		public IActionResult Tokens([FromQuery] UserTokens userTokens) => Ok(userTokens);

		[HttpGet]
		[Route("refresh")]
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

			if(!await _jwtAuthenticator.IsRefreshTokenValidAsync(id, refreshToken))
			{
				return BadRequest("Refresh token was expired");
			}

			var newToken = _jwtAuthenticator.GenerateAccessToken(principals.Claims);

			return Ok(newToken);
		}
	}
}
