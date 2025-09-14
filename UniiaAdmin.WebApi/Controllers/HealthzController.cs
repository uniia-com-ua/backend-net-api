using Microsoft.AspNetCore.Mvc;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;

namespace UniiaAdmin.WebApi.Controllers
{
    [Route("healthz")]
    [ApiController]
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
        public IActionResult Get()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Детальна перевірка готовності з перевіркою всіх БД та сервісів
        /// </summary>
        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
			var adminDbHealthy = await _healthCheckService.CanAdminConnectAsync();
			var appDbHealthy = await _healthCheckService.CanAppConnectAsync();
			var mongoHealthy = await _healthCheckService.CanMongoConnectAsync();

			var allHealthy = adminDbHealthy && appDbHealthy && mongoHealthy;

			return Ok(new
			{
				status = allHealthy ? "ready" : "degraded",
				timestamp = DateTime.UtcNow,
				databases = new
				{
					admin_postgresql = adminDbHealthy ? "healthy" : "unhealthy",
					application_postgresql = appDbHealthy ? "healthy" : "unhealthy",
					mongodb = mongoHealthy ? "healthy" : "unhealthy",
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
        /// Перевірка окремих компонентів
        /// </summary>
        [HttpGet("components")]
        public async Task<IActionResult> Components()
        {
            var components = new Dictionary<string, HealthCheckComponent>
			{
				["admin_db"] = _healthCheckService.GetHealthStatusAsync(await _healthCheckService.CanAdminConnectAsync()),
				["application_db"] = _healthCheckService.GetHealthStatusAsync(await _healthCheckService.CanAppConnectAsync()),
				["mongodb"] = _healthCheckService.GetHealthStatusAsync(await _healthCheckService.CanMongoConnectAsync())
			};

			return Ok(new
			{
				status = "components_checked",
				timestamp = DateTime.UtcNow,
				components
			});
		}
    }
}
