using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using UNIIAadminAPI.Enums;
using UNIIAadminAPI.Serializers;
using UNIIAadminAPI.Services;

namespace UNIIAadminAPI.Controllers
{
    [Route("health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
