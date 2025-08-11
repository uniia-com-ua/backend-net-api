using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.GridFS;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;

namespace UNIIAadminAPI.Controllers
{
    [Route("api/v1/healthz")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly AdminContext _adminContext;
        private readonly ApplicationContext _applicationContext;
        private readonly MongoDbContext _mongoContext;
        private readonly GridFSBucket _gridFsBucket;
        private readonly IHealthCheckService _healthCheckService;

        public HealthController(
            AdminContext adminContext, 
            ApplicationContext applicationContext,
            MongoDbContext mongoContext,
            GridFSBucket gridFsBucket,
            IHealthCheckService healthCheckService)
        {
            _adminContext = adminContext;
            _applicationContext = applicationContext;
            _mongoContext = mongoContext;
            _gridFsBucket = gridFsBucket;
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
			var adminDbHealthy = await _adminContext.Database.CanConnectAsync();
			var appDbHealthy = await _applicationContext.Database.CanConnectAsync();
			var mongoHealthy = await _mongoContext.Database.CanConnectAsync();

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
            var components = new Dictionary<string, HealthCheckComponent>();

			components["admin_db"] = _healthCheckService.GetHealthStatusAsync(await _adminContext.Database.CanConnectAsync());
			components["application_db"] = _healthCheckService.GetHealthStatusAsync(await _applicationContext.Database.CanConnectAsync());
			components["mongodb"] = _healthCheckService.GetHealthStatusAsync(await _mongoContext.Database.CanConnectAsync());

			return Ok(new
			{
				status = "components_checked",
				timestamp = DateTime.UtcNow,
				components
			});
		}
    }
}
