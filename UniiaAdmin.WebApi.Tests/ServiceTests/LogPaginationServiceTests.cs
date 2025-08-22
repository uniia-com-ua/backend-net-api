using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Services;
using Xunit;

namespace UniiaAdmin.WebApi.Tests.ServiceTests;

public class LogPaginationServiceTests
{
	private readonly Mock<IMongoUnitOfWork> _mongoMock;
	private readonly Mock<IPaginationService> _paginationMock;
	private readonly LogPaginationService _service;

	public LogPaginationServiceTests()
	{
		_mongoMock = new Mock<IMongoUnitOfWork>();
		_paginationMock = new Mock<IPaginationService>();

		_service = new LogPaginationService(_mongoMock.Object, _paginationMock.Object);
	}

	[Fact]
	public async Task GetPagedListAsync_ByUserId_CallsPaginationService()
	{
		// Arrange
		const string userId = "user1";
		const int skip = 0;
		const int take = 10;

		var queryable = new List<LogActionModel>().AsQueryable();
		_mongoMock.Setup(m => m.Query<LogActionModel>(It.IsAny<System.Linq.Expressions.Expression<System.Func<LogActionModel, bool>>>()))
				  .Returns(queryable);
		_paginationMock.Setup(p => p.GetPagedListAsync(queryable, skip, take))
					   .ReturnsAsync(new List<LogActionModel> { new LogActionModel { UserId = userId } });

		// Act
		var result = await _service.GetPagedListAsync(userId, skip, take);

		// Assert
		Assert.Single(result);
		Assert.Equal(userId, result[0].UserId);
		_paginationMock.Verify(p => p.GetPagedListAsync(queryable, skip, take), Times.Once);
	}

	[Fact]
	public async Task GetPagedListAsync_ByModelIdAndName_CallsPaginationService()
	{
		// Arrange
		const int modelId = 1;
		const string modelName = "TestModel";
		const int skip = 0;
		const int take = 5;

		var queryable = new List<LogActionModel>().AsQueryable();
		_mongoMock.Setup(m => m.Query<LogActionModel>(It.IsAny<System.Linq.Expressions.Expression<System.Func<LogActionModel, bool>>>()))
				  .Returns(queryable);
		_paginationMock.Setup(p => p.GetPagedListAsync(queryable, skip, take))
					   .ReturnsAsync(new List<LogActionModel> { new LogActionModel { ModelId = modelId, ModelName = modelName } });

		// Act
		var result = await _service.GetPagedListAsync(modelId, modelName, skip, take);

		// Assert
		Assert.Single(result);
		Assert.Equal(modelId, result[0].ModelId);
		Assert.Equal(modelName, result[0].ModelName);
		_paginationMock.Verify(p => p.GetPagedListAsync(queryable, skip, take), Times.Once);
	}

	[Fact]
	public async Task GetPagedListAsync_All_CallsPaginationService()
	{
		// Arrange
		const int skip = 0;
		const int take = 20;

		var queryable = new List<LogActionModel>().AsQueryable();
		_mongoMock.Setup(m => m.Query<LogActionModel>())
				  .Returns(queryable);
		_paginationMock.Setup(p => p.GetPagedListAsync(queryable, skip, take))
					   .ReturnsAsync(new List<LogActionModel> { new LogActionModel() });

		// Act
		var result = await _service.GetPagedListAsync(skip, take);

		// Assert
		Assert.Single(result);
		_paginationMock.Verify(p => p.GetPagedListAsync(queryable, skip, take), Times.Once);
	}
}
