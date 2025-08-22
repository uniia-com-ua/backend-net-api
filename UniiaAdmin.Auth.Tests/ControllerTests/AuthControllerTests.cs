namespace UniiaAdmin.Auth.Tests.ControllerTests;

using AutoMapper;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using UniiaAdmin.Auth.Controllers;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Auth.Tests;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Models;
using UniiaAdmin.Data.Models.AuthModels;
using Xunit;

public class AuthControllerTests
{
	private readonly ControllerWebAppFactory<AuthController> _factory;
	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public AuthControllerTests()
	{
		var mockProvider = new MockProvider();

		mockProvider.Mock<IJwtAuthenticator>();
		mockProvider.Mock<IAdminUserRepository>();
		mockProvider.Mock<IMongoUnitOfWork>();
		mockProvider.Mock<IMapper>();

		_factory = new ControllerWebAppFactory<AuthController>(mockProvider);
	}

	[Fact]
	public async Task Tokens_Returns200WithTokens()
	{
		// Arrange
		var client = _factory.CreateClient();
		var tokens = new UserTokens
		{
			AccessToken = "access123",
			RefreshToken = "refresh123"
		};

		// Act
		var response = await client.GetAsync($"/api/v1/tokens?accessToken={tokens.AccessToken}&refreshToken={tokens.RefreshToken}");
		var returned = await DeserializeResponse<UserTokens>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(tokens.AccessToken, returned!.AccessToken);
		Assert.Equal(tokens.RefreshToken, returned.RefreshToken);
	}

	[Fact]
	public async Task RefreshToken_InvalidAccessToken_Returns400()
	{
		// Arrange
		var mockedJwt = _factory.Mocks.Mock<IJwtAuthenticator>();
		mockedJwt.Setup(j => j.GetPrincipalFromExpiredToken(It.IsAny<string>()))
				 .ReturnsAsync((ClaimsPrincipal?)null);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/refresh?accessToken=bad&refreshToken=test");

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task RefreshToken_ValidTokens_Returns200()
	{
		// Arrange
		var mockedJwt = _factory.Mocks.Mock<IJwtAuthenticator>();
		var principal = new ClaimsPrincipal(new ClaimsIdentity(
		[
			new Claim(ClaimTypes.NameIdentifier, "user1")
		]));

		mockedJwt.Setup(j => j.GetPrincipalFromExpiredToken("access"))
				 .ReturnsAsync(principal);
		mockedJwt.Setup(j => j.IsRefreshTokenValidAsync("user1", "refresh"))
				 .ReturnsAsync(true);
		mockedJwt.Setup(j => j.GenerateAccessToken(It.IsAny<IEnumerable<Claim>>()))
				 .Returns("newAccess");

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/refresh?accessToken=access&refreshToken=refresh");
		var returned = await response.Content.ReadAsStringAsync();

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal("newAccess", returned);
	}

	[Fact]
	public async Task RefreshToken_InvalidId_Returns400()
	{
		// Arrange
		var mockedJwt = _factory.Mocks.Mock<IJwtAuthenticator>();
		var principal = new ClaimsPrincipal(new ClaimsIdentity([]));

		mockedJwt.Setup(j => j.GetPrincipalFromExpiredToken("access"))
				 .ReturnsAsync(principal);
		mockedJwt.Setup(j => j.IsRefreshTokenValidAsync("user1", "refresh"))
				 .ReturnsAsync(true);
		mockedJwt.Setup(j => j.GenerateAccessToken(It.IsAny<IEnumerable<Claim>>()))
				 .Returns("newAccess");

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/refresh?accessToken=access&refreshToken=refresh");
		var returned = await response.Content.ReadAsStringAsync();

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task RefreshToken_InvalidRefresh_Returns400()
	{
		// Arrange
		var mockedJwt = _factory.Mocks.Mock<IJwtAuthenticator>();
		var principal = new ClaimsPrincipal(new ClaimsIdentity(
		[
			new Claim(ClaimTypes.NameIdentifier, "user1")
		]));

		mockedJwt.Setup(j => j.GetPrincipalFromExpiredToken("access"))
				 .ReturnsAsync(principal);
		mockedJwt.Setup(j => j.IsRefreshTokenValidAsync("user1", "refresh"))
				 .ReturnsAsync(false);
		mockedJwt.Setup(j => j.GenerateAccessToken(It.IsAny<IEnumerable<Claim>>()))
				 .Returns("newAccess");

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/refresh?accessToken=access&refreshToken=refresh");
		var returned = await response.Content.ReadAsStringAsync();

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Theory]
	[InlineData(false, null)]
	[InlineData(true, null)]
	public async Task GetAuthUserPicture_FileNotFound_Returns404(bool init, byte[]? file)
	{
		var userPhoto = new AdminUserPhoto();

		if (init)
		{
			userPhoto.File = file;
		}

		// Arrange
		var mockedRepo = _factory.Mocks.Mock<IAdminUserRepository>();
		mockedRepo.Setup(r => r.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				  .ReturnsAsync(new AdminUser { ProfilePictureId = ObjectId.GenerateNewId().ToString() });

		var mockedMongo = _factory.Mocks.Mock<IMongoUnitOfWork>();
		mockedMongo.Setup(m => m.FindFileAsync<AdminUserPhoto>(It.IsAny<ObjectId>()))
				   .ReturnsAsync((AdminUserPhoto?)null);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/picture");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetAuthUserPicture_FileFound_Returns200()
	{
		var userPhoto = new AdminUserPhoto
		{
			Id = ObjectId.GenerateNewId(),
			File = [1, 2, 3]
		};

		// Arrange
		var mockedRepo = _factory.Mocks.Mock<IAdminUserRepository>();
		mockedRepo.Setup(r => r.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				  .ReturnsAsync(new AdminUser { ProfilePictureId = ObjectId.GenerateNewId().ToString() });

		var mockedMongo = _factory.Mocks.Mock<IMongoUnitOfWork>();
		mockedMongo.Setup(m => m.FindFileAsync<AdminUserPhoto>(It.IsAny<ObjectId>()))
				   .ReturnsAsync(userPhoto);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/picture");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task GetAuthUser_Success_Returns200()
	{
		// Arrange

		var user = new AdminUser
		{
			Id = ObjectId.GenerateNewId().ToString()
		};

		var userDto = new AdminUserDto
		{
			Id = user.Id
		};

		var mockedRepo = _factory.Mocks.Mock<IAdminUserRepository>();
		mockedRepo.Setup(r => r.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				  .ReturnsAsync(user);

		var mockedMongo = _factory.Mocks.Mock<IMapper>();
		mockedMongo.Setup(m => m.Map<AdminUserDto>(It.IsAny<AdminUser>()))
				   .Returns(userDto);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/user");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task GetLogHistory_Returns200()
	{
		// Arrange
		var mockedMongo = _factory.Mocks.Mock<IMongoUnitOfWork>();
		mockedMongo.Setup(m => m.GetLogInHistory(It.IsAny<string?>(), 0, 10))
				   .ReturnsAsync(
				   [
					   new() {
						   UserId = "user1",
						   LogInTime = DateTime.UtcNow,
						   IpAdress = "",
						   LogInType = "",
						   UserAgent = "test-agent",
					   }
				   ]);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/log-history");
		var returned = await DeserializeResponse<List<AdminLogInHistory>>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!);
	}

	private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
	{
		var json = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<T>(json, _jsonOptions);
	}
}
