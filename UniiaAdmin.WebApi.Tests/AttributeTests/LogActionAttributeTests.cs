using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using System.Security.Claims;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using Xunit;

namespace UniiaAdmin.WebApi.Tests.AttributeTests
{
	public class LogActionAttributeTests
	{
		[Fact]
		public async Task OnActionExecutionAsync_ShouldLogAction_WhenUserAndIdExist()
		{
			// Arrange
			var mongoUowMock = new Mock<IMongoUnitOfWork>();
			mongoUowMock.Setup(u => u.AddAsync<LogActionModel>(It.IsAny<LogActionModel>()))
						.Returns(Task.CompletedTask);

			var httpContext = new DefaultHttpContext
			{
				RequestServices = Mock.Of<IServiceProvider>(sp =>
					sp.GetService(typeof(IMongoUnitOfWork)) == mongoUowMock.Object
				),
				User = new ClaimsPrincipal(new ClaimsIdentity(
					[new Claim(ClaimTypes.NameIdentifier, "user123")]
				))
			};

			var actionContext = new ActionContext(
				httpContext,
				new RouteData(),
				new ActionDescriptor(),
				new ModelStateDictionary()
			);

			var executingContext = new ActionExecutingContext(
				actionContext,
				[],
				new Dictionary<string, object>
					{
						{ "id", 123 }
					}!,
				new object()
			);

			var executedContext = new ActionExecutedContext(
				actionContext,
				[],
				new object()
			);

			var filter = new LogActionAttribute("publication", "update");

			// Act
			await filter.OnActionExecutionAsync(executingContext, () => Task.FromResult(executedContext));

			// Assert
			mongoUowMock.Verify(u => u.AddAsync(It.IsAny<LogActionModel>()), Times.Once);
			mongoUowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
		}

		[Fact]
		public async Task OnActionExecutionAsync_ShouldDoNothing_WhenNoUserId()
		{
			// Arrange
			var mongoUowMock = new Mock<IMongoUnitOfWork>();
			var httpContext = new DefaultHttpContext
			{
				RequestServices = Mock.Of<IServiceProvider>(sp =>
					sp.GetService(typeof(IMongoUnitOfWork)) == mongoUowMock.Object
				),
				User = new ClaimsPrincipal()
			};

			var actionContext = new ActionContext(
				httpContext,
				new RouteData(),
				new ActionDescriptor(),
				new ModelStateDictionary()
			);

			var executingContext = new ActionExecutingContext(
				actionContext,
				[],
				new Dictionary<string, object>()!,
				new object()
			);

			var executedContext = new ActionExecutedContext(
				actionContext,
				[],
				new object()
			);

			var filter = new LogActionAttribute("publication", "update");

			// Act
			await filter.OnActionExecutionAsync(executingContext, () => Task.FromResult(executedContext));

			// Assert
			mongoUowMock.Verify(u => u.AddAsync(It.IsAny<LogActionModel>()), Times.Never);
			mongoUowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
		}

		[Fact]
		public async Task OnActionExecutionAsync_ShouldDoNothing_WhenIdMissing()
		{
			// Arrange
			var mongoUowMock = new Mock<IMongoUnitOfWork>();
			var httpContext = new DefaultHttpContext
			{
				RequestServices = Mock.Of<IServiceProvider>(sp =>
					sp.GetService(typeof(IMongoUnitOfWork)) == mongoUowMock.Object
				),
				User = new ClaimsPrincipal(new ClaimsIdentity(
					[new Claim(ClaimTypes.NameIdentifier, "user123")]
				))
			};

			var actionContext = new ActionContext(
				httpContext,
				new RouteData(),
				new ActionDescriptor(),
				new ModelStateDictionary()
			);

			var executingContext = new ActionExecutingContext(
				actionContext,
				[],
				new Dictionary<string, object>()!,
				new object()
			);

			var executedContext = new ActionExecutedContext(
				actionContext,
				[],
				new object()
			);

			var filter = new LogActionAttribute("publication", "update");

			// Act
			await filter.OnActionExecutionAsync(executingContext, () => Task.FromResult(executedContext));

			// Assert
			mongoUowMock.Verify(u => u.AddAsync(It.IsAny<LogActionModel>()), Times.Never);
			mongoUowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
		}
	}
}
