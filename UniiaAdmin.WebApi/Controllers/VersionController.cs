using Microsoft.AspNetCore.Mvc;

namespace UniiaAdmin.WebApi.Controllers;

/// <summary>
/// Короткий endpoint для версії
/// </summary>
[ApiController]
[Route("ver")]
public class VersionController : ControllerBase
{
	/// <summary>
	/// Короткий endpoint для отримання версії
	/// </summary>
	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public IActionResult Get()
	{
		var version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "unknown";

		return Ok(new { version });
	}
}
