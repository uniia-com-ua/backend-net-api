using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.WebApi.Interfaces;
using UNIIAadminAPI.Controllers;

namespace UniiaAdmin.Tests;

public class ControllerWebAppFactory<T> : WebApplicationFactory<AuthorController>
{
	public readonly MockProvider Mocks;

	public ControllerWebAppFactory(MockProvider mocks)
	{
		Mocks = mocks;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		base.ConfigureWebHost(builder);

		builder.ConfigureServices(services =>
		{
			foreach (var mockPair in Mocks)
			{
				var descriptor = services.FirstOrDefault(d => d.ServiceType == mockPair.Key);
				if (descriptor != null)
					services.Remove(descriptor);

				services.AddSingleton(mockPair.Key, mockPair.Value.Object);
			}

			var dbInitializerMock = new Moq.Mock<IDatabaseInitilizerService>();
			dbInitializerMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);

			var existingDbInit = services.SingleOrDefault(d => d.ServiceType == typeof(IDatabaseInitilizerService));
			if (existingDbInit != null)
				services.Remove(existingDbInit);

			services.AddSingleton(dbInitializerMock.Object);

			services.AddAuthorization(options =>
			{
				options.DefaultPolicy = new AuthorizationPolicyBuilder()
					.RequireAssertion(_ => true)
					.Build();

				options.FallbackPolicy = new AuthorizationPolicyBuilder()
					.RequireAssertion(_ => true)
					.Build();

				foreach (var modelName in Enum.GetValues(typeof(PermissionResource)))
				{
					foreach (var action in Enum.GetValues(typeof(CrudActions)))
					{
						var policyName = $"{modelName}.{action}";
						options.AddPolicy(policyName, policy => policy.RequireAssertion(_ => true));
					}
				}
			});

			services.AddAuthentication("Test")
					.AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

			services.PostConfigure<AuthenticationOptions>(options =>
			{
				options.DefaultAuthenticateScheme = "Test";
				options.DefaultChallengeScheme = "Test";
			});
		});

		builder.ConfigureAppConfiguration((context, config) =>
		{
			var dict = new Dictionary<string, string?>
			{
				{"JWT:ValidIssuer", "TestIssuer"},
				{"JWT:ValidAudience", "TestAudience"}
			};

			config.AddInMemoryCollection(dict);
		});

		Environment.SetEnvironmentVariable("JWT_TOKEN_KEY", "TestSecretKey123456");
	}
}