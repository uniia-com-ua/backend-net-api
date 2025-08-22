using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UniiaAdmin.Auth.Interfaces;
using UniiaAdmin.Auth.Services;
using UniiaAdmin.Data.Models;
using Xunit;

namespace UniiaAdmin.Auth.Tests.ServiceTests
{
	public class JwtValidationServiceTests
	{
		private readonly Mock<IConfiguration> _mockConfig;
		private readonly Mock<IAdminUserRepository> _mockRepo;
		private readonly JwtValidationService _service;

		public JwtValidationServiceTests()
		{
			_mockConfig = new Mock<IConfiguration>();
			_mockRepo = new Mock<IAdminUserRepository>();

			_mockConfig.Setup(c => c["JWT:ValidIssuer"]).Returns("TestIssuer");
			_mockConfig.Setup(c => c["JWT:ValidAudience"]).Returns("TestAudience");
			_mockConfig.Setup(c => c["JWT:TokenValidityInMinutes"]).Returns("60");
			_mockConfig.Setup(c => c["JWT:RefreshTokenValidityInDays"]).Returns("30");

			Environment.SetEnvironmentVariable("JWT_TOKEN_KEY", "TestSecretKey1234567891011121314");

			_service = new JwtValidationService(_mockConfig.Object, _mockRepo.Object);
		}

		[Fact]
		public void GenerateAccessToken_WithClaims_ReturnsToken()
		{
			// Arrange
			var claims = new List<Claim> { new Claim(ClaimTypes.Name, "TestUser") };

			// Act
			var token = _service.GenerateAccessToken(claims);

			// Assert
			Assert.False(string.IsNullOrEmpty(token));
		}

		[Fact]
		public void GenerateAccessToken_NullClaims_ReturnsEmpty()
		{
			// Act
			var token = _service.GenerateAccessToken(null);

			// Assert
			Assert.Equal(string.Empty, token);
		}

		[Fact]
		public void GenerateRefreshToken_ReturnsNonEmptyString()
		{
			// Act
			var refreshToken = _service.GenerateRefreshToken();

			// Assert
			Assert.False(string.IsNullOrEmpty(refreshToken));
		}

		[Fact]
		public async Task GetPrincipalFromExpiredToken_ValidToken_ReturnsClaimsPrincipal()
		{
			// Arrange

			var claims = new[]
			{
				new Claim(ClaimTypes.Name, "TestUser"),
				new Claim(ClaimTypes.Role, "Admin")
			};

			var tokenString = _service.GenerateAccessToken(claims);

			// Act
			var principal = await _service.GetPrincipalFromExpiredToken(tokenString);

			// Assert
			Assert.NotNull(principal);
			Assert.Contains(principal!.Claims, c => c.Type == ClaimTypes.Name && c.Value == "TestUser");
			Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
		}

		[Fact]
		public async Task GetPrincipalFromExpiredToken_InvalidToken_ReturnsNull()
		{
			// Arrange
			var invalidToken = "not-a-valid-token";

			// Act
			var principal = await _service.GetPrincipalFromExpiredToken(invalidToken);

			// Assert
			Assert.Null(principal);
		}

		[Fact]
		public async Task SaveRefreshTokenAsync_CallsRepositoryMethods()
		{
			// Arrange
			var user = new AdminUser { Id = "user1" };
			var refreshToken = "refresh123";

			_mockRepo.Setup(r => r.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
			_mockRepo.Setup(r => r.SetAuthenticationTokenAsync(user, It.IsAny<string>(), "refresh_token", refreshToken))
					 .ReturnsAsync(IdentityResult.Success);

			// Act
			await _service.SaveRefreshTokenAsync(user, refreshToken);

			// Assert
			_mockRepo.Verify(r => r.UpdateAsync(user), Times.Once);
			_mockRepo.Verify(r => r.SetAuthenticationTokenAsync(user, It.IsAny<string>(), "refresh_token", refreshToken), Times.Once);
		}

		[Fact]
		public async Task IsRefreshTokenValidAsync_ReturnsFalse_WhenUserNotFound()
		{
			// Arrange
			_mockRepo.Setup(r => r.FindByIdAsync("user1")).ReturnsAsync((AdminUser?)null);

			// Act
			var result = await _service.IsRefreshTokenValidAsync("user1", "token");

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task IsRefreshTokenValidAsync_ReturnsFalse_WhenTokenMismatch()
		{
			// Arrange
			var user = new AdminUser { Id = "user1", RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1).ToString() };
			_mockRepo.Setup(r => r.FindByIdAsync("user1")).ReturnsAsync(user);
			_mockRepo.Setup(r => r.GetAuthenticationTokenAsync(user, It.IsAny<string>(), "refresh_token")).ReturnsAsync("wrongToken");

			// Act
			var result = await _service.IsRefreshTokenValidAsync("user1", "token");

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task IsRefreshTokenValidAsync_ReturnsTrue_WhenTokenMatches()
		{
			// Arrange
			var user = new AdminUser
			{
				Id = "user1",
				RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1).ToString("MM/dd/yyyy HH:mm:ss")
			};

			_mockRepo.Setup(r => r.FindByIdAsync("user1")).ReturnsAsync(user);
			_mockRepo.Setup(r => r.GetAuthenticationTokenAsync(user, It.IsAny<string>(), "refresh_token")).ReturnsAsync("token");

			// Act
			var result = await _service.IsRefreshTokenValidAsync("user1", "token");

			// Assert
			Assert.True(result);
		}
	}
}
