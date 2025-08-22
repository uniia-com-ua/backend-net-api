using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniiaAdmin.Auth.Controllers;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;

namespace UniiaAdmin.Auth.Tests;

public class ControllerWebAppFactory<T> : WebApplicationFactory<AuthController>
{
	public readonly MockProvider Mocks;

	public ControllerWebAppFactory(MockProvider mocks)
	{
		Mocks = mocks;
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		base.ConfigureWebHost(builder);

		Environment.SetEnvironmentVariable("JWT_TOKEN_KEY", "TestSecretKey123456");
		Environment.SetEnvironmentVariable("OAUTH2_CLIENT_ID", "test-client-id");
		Environment.SetEnvironmentVariable("OAUTH2_CLIENT_SECRET", "test-client-secret");

		builder.ConfigureServices(services =>
		{
			foreach (var mockPair in Mocks)
			{
				var descriptor = services.FirstOrDefault(d => d.ServiceType == mockPair.Key);
				if (descriptor != null)
					services.Remove(descriptor);

				services.AddSingleton(mockPair.Key, mockPair.Value.Object);
			}

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
				{"JWT:ValidAudience", "TestAudience"},
			};

			config.AddInMemoryCollection(dict);

			context.HostingEnvironment.EnvironmentName = CustomEnviroments.Test;
		});

		Environment.SetEnvironmentVariable("JWT_TOKEN_KEY", "TestSecretKey123456");
	}
}