using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Data;

namespace UniiaAdmin.Auth.Controllers
{
    [Route("healthz")]
    [ApiController]
    public class HealthzController : ControllerBase
    {
        private readonly AdminContext _adminContext;
        private readonly MongoDbContext _mongoContext;

        public HealthzController(AdminContext adminContext, MongoDbContext mongoContext)
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
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Детальна перевірка готовності з перевіркою БД
        /// </summary>
        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                // Перевірка PostgreSQL підключення
                await _adminContext.Database.CanConnectAsync();
                
                // Перевірка MongoDB підключення
                var mongoCanConnect = await _mongoContext.Database.CanConnectAsync();

                return Ok(new 
                { 
                    status = "ready", 
                    timestamp = DateTime.UtcNow,
                    databases = new 
                    {
                        postgresql = "healthy",
                        mongodb = mongoCanConnect ? "healthy" : "warning"
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
    }
} 