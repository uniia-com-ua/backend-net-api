using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.Data.Models.AuthModels;

namespace UniiaAdmin.Auth.Controllers;

[ApiController]
[Route("healthz")]
public class HealthzController : ControllerBase
{
	private readonly IHealthCheckService _healthCheckService;

	public HealthzController(IHealthCheckService healthCheckService)
	{
		_healthCheckService = healthCheckService;
	}

	/// <summary>
	/// Базова перевірка працездатності сервісу
	/// </summary>
	[HttpGet]
	[ProducesResponseType(typeof(HealthCheckComponent), StatusCodes.Status200OK)]
	public IActionResult Get()
	{
		return Ok(new HealthCheckComponent { Status = "healthy", Timestamp = DateTime.UtcNow });
	}

	/// <summary>
	/// Детальна перевірка готовності з перевіркою БД
	/// </summary>
	[HttpGet("ready")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<IActionResult> Ready()
	{
		var postgresCanConnect = await _healthCheckService.CanAdminConnectAsync();

		var mongoCanConnect = await _healthCheckService.CanMongoConnectAsync();

		return Ok(new
		{
			status = "ready",
			timestamp = DateTime.UtcNow,
			databases = new
			{
				postgresql = postgresCanConnect ? "healthy" : "warning",
				mongodb = mongoCanConnect ? "healthy" : "warning"
			}
		});
	}

	/// <summary>
	/// Швидка перевірка живучості
	/// </summary>
	[HttpGet("live")]
	[ProducesResponseType(typeof(HealthCheckComponent), StatusCodes.Status200OK)]
	public IActionResult Live()
	{
		return Ok(new HealthCheckComponent { Status = "alive", Timestamp = DateTime.UtcNow });
	}

	/// <summary>
	/// Повертає версію додатку з ConfigMap
	/// </summary>
	[HttpGet("version")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public IActionResult Version()
	{
		var version = Environment.GetEnvironmentVariable("APP_VERSION") ?? "unknown";

		var service = Assembly.GetExecutingAssembly().GetName().Name;

		return Ok(new
		{
			version,
			service,
			timestamp = DateTime.UtcNow
		});
	}
}
