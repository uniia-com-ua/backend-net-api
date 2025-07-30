namespace UniiaAdmin.WebApi.Services;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;

public class HealthCheckService : IHealthCheckService
{
	public HealthCheckComponent GetHealthStatusAsync(bool isHealthy)
	{
		return new HealthCheckComponent
		{
			Status = isHealthy ? "healthy" : "unhealthy",
			Timestamp = DateTime.UtcNow,
		};
	}
}
