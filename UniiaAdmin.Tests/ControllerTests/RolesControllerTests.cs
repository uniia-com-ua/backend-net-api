namespace UniiaAdmin.Tests.ControllerTests;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.WebApi.Controllers;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Resources;
using UNIIAadminAPI.Controllers;
using Xunit;

public class RolesControllerTests
{
	private readonly ControllerWebAppFactory<RolesController> _factory;
	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public RolesControllerTests()
	{
		var mockProvider = new MockProvider();

		var roleRepoMock = mockProvider.Mock<IRoleRepository>();

		var localizerMock = new Mock<IStringLocalizer<ErrorMessages>>();
		localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
					 .Returns((string key, object[] args) =>
						 new LocalizedString(key, $"{key} {string.Join(", ", args)}"));

		mockProvider.Mock<IRolePaginationService>();

		_factory = new ControllerWebAppFactory<RolesController>(mockProvider);
	}

	[Fact]
	public async Task CreateRole_ExistingRole_ReturnsBadRequest()
	{
		var client = _factory.CreateClient();
		var roleRepo = _factory.Mocks.Mock<IRoleRepository>();
		roleRepo.Setup(r => r.RoleExistsAsync("admin")).ReturnsAsync(true);

		var response = await client.PostAsync("/api/v1/roles?roleName=admin", null);
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task CreateRole_NewRole_ReturnsOk()
	{
		var client = _factory.CreateClient();
		var roleRepo = _factory.Mocks.Mock<IRoleRepository>();
		roleRepo.Setup(r => r.RoleExistsAsync("newRole")).ReturnsAsync(false);
		roleRepo.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

		var response = await client.PostAsync("/api/v1/roles?roleName=newRole", null);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task GetRoleByName_NotFound_Returns404()
	{
		var client = _factory.CreateClient();
		var roleRepo = _factory.Mocks.Mock<IRoleRepository>();
		roleRepo.Setup(r => r.FindByNameAsync("unknown")).ReturnsAsync((IdentityRole)null!);

		var response = await client.GetAsync("/api/v1/roles/unknown");
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetRoleByName_Found_Returns200()
	{
		var client = _factory.CreateClient();
		var roleRepo = _factory.Mocks.Mock<IRoleRepository>();
		var role = new IdentityRole("role");
		roleRepo.Setup(r => r.FindByNameAsync("role")).ReturnsAsync(role);

		var response = await client.GetAsync("/api/v1/roles/role");
		var returnedRole = await DeserializeResponse<IdentityRole>(response);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(role.Name, returnedRole!.Name);
	}

	[Fact]
	public async Task AddClaimToRole_ClaimExists_ReturnsConflict()
	{
		var client = _factory.CreateClient();
		var roleRepo = _factory.Mocks.Mock<IRoleRepository>();
		var role = new IdentityRole("role");
		roleRepo.Setup(r => r.FindByNameAsync("role")).ReturnsAsync(role);
		roleRepo.Setup(r => r.GetClaimsAsync(role))
				.ReturnsAsync(new List<Claim> { new("Permission", "claim") });

		var response = await client.PostAsync("/api/v1/roles/role/claim", null);
		Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
	}

	[Fact]
	public async Task AddClaimToRole_NewClaim_ReturnsOk()
	{
		var client = _factory.CreateClient();
		var roleRepo = _factory.Mocks.Mock<IRoleRepository>();
		var role = new IdentityRole("role");
		roleRepo.Setup(r => r.FindByNameAsync("role")).ReturnsAsync(role);
		roleRepo.Setup(r => r.GetClaimsAsync(role)).ReturnsAsync(new List<Claim>());
		roleRepo.Setup(r => r.AddClaimAsync(role, It.IsAny<Claim>())).ReturnsAsync(IdentityResult.Success);

		var response = await client.PostAsync("/api/v1/roles/role/claim", null);
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task DeleteRole_AdminRole_ReturnsBadRequest()
	{
		var client = _factory.CreateClient();
		var response = await client.DeleteAsync($"/api/v1/roles?roleName={CustomRoles.AdminRole}");

		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
	{
		var json = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<T>(json, _jsonOptions);
	}
}
