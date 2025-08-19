namespace UniiaAdmin.WebApi.Tests.RepositoryTests;

using AutoMapper;
using Moq;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Repository;
using Xunit;

public class GenericRepositoryTests
{
	private readonly Mock<IApplicationUnitOfWork> _uowMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly GenericRepository _repository;

	public GenericRepositoryTests()
	{
		_uowMock = new Mock<IApplicationUnitOfWork>();
		_mapperMock = new Mock<IMapper>();

		_repository = new GenericRepository(_uowMock.Object, _mapperMock.Object);
	}

	private class TestEntity : IEntity
	{
		public int Id { get; set; }
		public string? Name { get; set; }
	}

	[Fact]
	public async Task CreateAsync_SetsIdToZeroAndSaves()
	{
		// Arrange
		var entity = new TestEntity { Id = 42, Name = "Test" };

		_uowMock.Setup(u => u.AddAsync(entity)).Returns(Task.CompletedTask);

		// Act
		await _repository.CreateAsync(entity);

		// Assert
		Assert.Equal(0, entity.Id);
		_uowMock.Verify(u => u.AddAsync(entity), Times.Once);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task UpdateAsync_MapsAndSaves()
	{
		// Arrange
		var existing = new TestEntity { Id = 1, Name = "Old" };
		var updated = new TestEntity { Id = 1, Name = "New" };

		// Act
		await _repository.UpdateAsync(updated, existing);

		// Assert
		_mapperMock.Verify(m => m.Map(updated, existing), Times.Once);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task DeleteAsync_RemovesAndSaves()
	{
		// Arrange
		var entity = new TestEntity { Id = 1, Name = "ToDelete" };

		// Act
		await _repository.DeleteAsync(entity);

		// Assert
		_uowMock.Verify(u => u.Remove(entity), Times.Once);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}
}
