using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.WebApi.Controllers;

/// <summary>
/// Короткий endpoint для версії
/// </summary>
[ApiController]
[Route("api/v1/ver")]
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
