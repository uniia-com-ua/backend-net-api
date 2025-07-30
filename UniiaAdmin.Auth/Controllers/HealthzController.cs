using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.Auth.Controllers;

[Route("healthz")]
[ApiController]
public class HealthzController : ControllerBase
{
	private readonly AdminContext _adminContext;
	private readonly MongoDbContext _mongoContext;

	public HealthzController
		(AdminContext adminContext,
		MongoDbContext mongoContext)
	{
		_adminContext = adminContext;
		_mongoContext = mongoContext;
	}

	/// <summary>
	/// Базова перевірка працездатності сервісу
	/// </summary>
	[HttpGet]
	public IActionResult Get()
	{
		return Ok(new HealthCheckComponent { Status = "healthy", Timestamp = DateTime.UtcNow });
	}

	/// <summary>
	/// Детальна перевірка готовності з перевіркою БД
	/// </summary>
	[HttpGet("ready")]
	public async Task<IActionResult> Ready()
	{
		var postgresCanConnect = await _adminContext.Database.CanConnectAsync();

		var mongoCanConnect = await _mongoContext.Database.CanConnectAsync();

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
	public IActionResult Live()
	{
		return Ok(new HealthCheckComponent { Status = "alive", Timestamp = DateTime.UtcNow });
	}

	/// <summary>
	/// Повертає версію додатку з ConfigMap
	/// </summary>
	[HttpGet("version")]
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
