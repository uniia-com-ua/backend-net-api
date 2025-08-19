using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using UniiaAdmin.WebApi.Services;
using Xunit;

namespace UniiaAdmin.WebApi.Tests.ServiceTests;

public class PaginationServiceTests
{
	private readonly PaginationService _service;

	public PaginationServiceTests()
	{
		var configSectionMock = new Mock<IConfigurationSection>();
		configSectionMock.Setup(s => s.Value).Returns("5");

		var configMock = new Mock<IConfiguration>();
		configMock.Setup(c => c.GetSection("PageSettings:MaxPageSize")).Returns(configSectionMock.Object);

		_service = new PaginationService(configMock.Object);
	}

	public class TestEntity
	{
		public int Id { get; set; }
	}

	[Fact]
	public async Task GetPagedListAsync_ReturnsEmpty_WhenQueryIsNull()
	{
		// Arrange
		IQueryable<object>? query = null;
		int skip = 0;
		int take = 10;

		// Act
		var result = await _service.GetPagedListAsync(query, skip, take);

		// Assert
		Assert.Empty(result);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(0, -5)]
	public async Task GetPagedListAsync_ReturnsEmpty_WhenTakeIsZeroOrNegative(int skip, int take)
	{
		// Arrange
		var query = new List<int> { 1, 2, 3 }.AsQueryable();

		// Act
		var resultZero = await _service.GetPagedListAsync<int>(query, skip, take);

		// Assert
		Assert.Empty(resultZero);
	}

	[Fact]
	public async Task GetPagedListAsync_RespectsMaxPageSize()
	{
		// Arrange
		var query = Enumerable.Range(1, 10).AsQueryable();
		int skip = 0;
		int take = 10;

		// Act
		var result = await _service.GetPagedListAsync(query, skip, take);

		// Assert
		Assert.Equal(5, result.Count);
		Assert.Equal(new List<int> { 1, 2, 3, 4, 5 }, result);
	}

	[Fact]
	public async Task GetPagedListAsync_ReturnsCorrectSubset()
	{
		// Arrange
		var query = Enumerable.Range(1, 10).AsQueryable();
		int skip = 2;
		int take = 3;

		// Act
		var result = await _service.GetPagedListAsync(query, skip, take);

		// Assert
		Assert.Equal(3, result.Count);
		Assert.Equal(new List<int> { 3, 4, 5 }, result);
	}

	[Fact]
	public async Task GetPagedListAsync_TakeLessThanMax_ReturnsRequestedCount()
	{
		// Arrange
		var query = Enumerable.Range(1, 10).AsQueryable();
		int skip = 0;
		int take = 3;

		// Act
		var result = await _service.GetPagedListAsync(query, skip, take);

		// Assert
		Assert.Equal(3, result.Count);
		Assert.Equal(new List<int> { 1, 2, 3 }, result);
	}
}
