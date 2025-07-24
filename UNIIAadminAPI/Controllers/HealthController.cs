using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using UniiaAdmin.Data.Data;

namespace UNIIAadminAPI.Controllers
{
    [Route("healthz")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly AdminContext _adminContext;
        private readonly ApplicationContext _applicationContext;
        private readonly MongoDbContext _mongoContext;
        private readonly GridFSBucket _gridFsBucket;

        public HealthController(
            AdminContext adminContext, 
            ApplicationContext applicationContext,
            MongoDbContext mongoContext,
            GridFSBucket gridFsBucket)
        {
            _adminContext = adminContext;
            _applicationContext = applicationContext;
            _mongoContext = mongoContext;
            _gridFsBucket = gridFsBucket;
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
            try
            {
                // Перевірка PostgreSQL підключень
                var adminDbHealthy = await _adminContext.Database.CanConnectAsync();
                var appDbHealthy = await _applicationContext.Database.CanConnectAsync();
                
                // Перевірка MongoDB підключення
                var mongoHealthy = await _mongoContext.Database.CanConnectAsync();

                // Перевірка GridFS
                var gridFsHealthy = true;
                try
                {
                    await _gridFsBucket.Database.RunCommandAsync<object>("{ ping: 1 }");
                }
                catch
                {
                    gridFsHealthy = false;
                }

                var allHealthy = adminDbHealthy && appDbHealthy && mongoHealthy && gridFsHealthy;

                return Ok(new 
                { 
                    status = allHealthy ? "ready" : "degraded", 
                    timestamp = DateTime.UtcNow,
                    databases = new 
                    {
                        admin_postgresql = adminDbHealthy ? "healthy" : "unhealthy",
                        application_postgresql = appDbHealthy ? "healthy" : "unhealthy",
                        mongodb = mongoHealthy ? "healthy" : "unhealthy",
                        gridfs = gridFsHealthy ? "healthy" : "unhealthy"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new 
                { 
                    status = "unhealthy", 
                    timestamp = DateTime.UtcNow,
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Швидка перевірка живучості
        /// </summary>
        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Перевірка окремих компонентів
        /// </summary>
        [HttpGet("components")]
        public async Task<IActionResult> Components()
        {
            var components = new Dictionary<string, object>();

            try
            {
                components["admin_db"] = await CheckComponent(() => _adminContext.Database.CanConnectAsync());
                components["application_db"] = await CheckComponent(() => _applicationContext.Database.CanConnectAsync());
                components["mongodb"] = await CheckComponent(() => _mongoContext.Database.CanConnectAsync());
                components["gridfs"] = await CheckComponent(async () => 
                {
                    await _gridFsBucket.Database.RunCommandAsync<object>("{ ping: 1 }");
                    return true;
                });

                return Ok(new 
                { 
                    status = "components_checked", 
                    timestamp = DateTime.UtcNow,
                    components 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    status = "error", 
                    timestamp = DateTime.UtcNow,
                    error = ex.Message,
                    components 
                });
            }
        }

        private async Task<object> CheckComponent(Func<Task<bool>> healthCheck)
        {
            try
            {
                var isHealthy = await healthCheck();
                return new { status = isHealthy ? "healthy" : "unhealthy", timestamp = DateTime.UtcNow };
            }
            catch (Exception ex)
            {
                return new { status = "error", timestamp = DateTime.UtcNow, error = ex.Message };
            }
        }
    }
}
