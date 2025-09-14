using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Moq;
using System.Threading.Tasks;
using UniiaAdmin.Data.Models;
using UNIIAadmin.Auth.Services;
using UniiaAdmin.Data.Data;
using Xunit;
using UniiaAdmin.Auth.Interfaces;

namespace UniiaAdmin.Auth.Tests.ServiceTests
{
	public class AuthServiceTests
	{
		private readonly Mock<IMongoUnitOfWork> _mockedMongo;
		private readonly AuthService _authService;

		public AuthServiceTests()
		{
			_mockedMongo = new Mock<IMongoUnitOfWork>();
			_authService = new AuthService(_mockedMongo.Object);
		}

		[Fact]
		public async Task AddLoginInfoToHistory_CallsAddAndSave()
		{
			// Arrange
			var adminUser = new AdminUser { Id = ObjectId.GenerateNewId().ToString() };

			var context = new DefaultHttpContext();
			context.Request.Headers["X-Real-IP"] = "127.0.0.1";
			context.Request.Headers["User-Agent"] = "test-agent";

			AdminLogInHistory? capturedLogInHistory = null;
			_mockedMongo.Setup(m => m.AddAsync(It.IsAny<AdminLogInHistory>()))
						.Callback<AdminLogInHistory>(log => capturedLogInHistory = log)
						.Returns(Task.CompletedTask);

			_mockedMongo.Setup(m => m.SaveChangesAsync())
						.Returns(Task.CompletedTask);

			// Act
			await _authService.AddLoginInfoToHistory(adminUser, context);

			// Assert
			_mockedMongo.Verify(m => m.AddAsync(It.IsAny<AdminLogInHistory>()), Times.Once);
			_mockedMongo.Verify(m => m.SaveChangesAsync(), Times.Once);

			Assert.NotNull(capturedLogInHistory);
			Assert.Equal(adminUser.Id, capturedLogInHistory!.UserId);
			Assert.Equal("127.0.0.1", capturedLogInHistory.IpAdress);
			Assert.Equal("test-agent", capturedLogInHistory.UserAgent);
			Assert.Equal("Credential login", capturedLogInHistory.LogInType);
			Assert.True(capturedLogInHistory.LogInTime <= DateTime.UtcNow);
		}
	}
}
