namespace UniiaAdmin.WebApi.Services;

using System.Threading.Tasks;
using UniiaAdmin.Data.Data;
using UniiaAdmin.WebApi.Interfaces;

public class DatabaseInitializerService : IDatabaseInitilizerService
{
	private readonly AdminContext _adminContext;

	private readonly ApplicationContext _applicationContext;

	public DatabaseInitializerService(
		AdminContext adminContext,
		ApplicationContext applicationContext)
	{
		_adminContext = adminContext;
		_applicationContext = applicationContext;
	}

	public async Task InitializeAsync()
	{
		await _adminContext.Database.EnsureCreatedAsync();

		await _applicationContext.Database.EnsureCreatedAsync();
	}
}
