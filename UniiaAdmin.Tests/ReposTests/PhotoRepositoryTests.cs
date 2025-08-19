namespace UniiaAdmin.WebApi.Tests.RepositoryTests;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Moq;
using System.Net.Mime;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Repository;
using Xunit;

public class PhotoRepositoryTests
{
	private readonly Mock<IApplicationUnitOfWork> _uowMock;
	private readonly Mock<IFileEntityService> _fileServiceMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly PhotoRepository _repository;

	public PhotoRepositoryTests()
	{
		_uowMock = new Mock<IApplicationUnitOfWork>();
		_fileServiceMock = new Mock<IFileEntityService>();
		_mapperMock = new Mock<IMapper>();
		_repository = new PhotoRepository(_uowMock.Object, _mapperMock.Object, _fileServiceMock.Object);
	}

	private class TestPhotoEntity : IPhotoEntity
	{
		public int Id { get; set; }
		public string? PhotoId { get; set; }
	}

	private class TestMongoPhoto : IMongoFileEntity
	{
		public ObjectId Id { get; set; } = default!;
		public byte[]? File { get; set; }
	}

	[Fact]
	public async Task CreateAsync_WithPhoto_SavesFileAndEntity()
	{
		// Arrange
		var entity = new TestPhotoEntity();
		var fileMock = new Mock<IFormFile>();
		var mongoPhoto = new TestMongoPhoto { Id = ObjectId.GenerateNewId() };

		_fileServiceMock
			.Setup(f => f.SaveFileAsync<TestMongoPhoto>(fileMock.Object, MediaTypeNames.Image.Jpeg))
			.ReturnsAsync(Result<TestMongoPhoto>.Success(mongoPhoto));

		// Act
		var result = await _repository.CreateAsync<TestPhotoEntity, TestMongoPhoto>(entity, fileMock.Object);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(mongoPhoto.Id.ToString(), entity.PhotoId);
		_uowMock.Verify(u => u.AddAsync(entity), Times.Once);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task CreateAsync_WithoutPhoto_SavesEntityOnly()
	{
		// Arrange
		var entity = new TestPhotoEntity();

		// Act
		var result = await _repository.CreateAsync<TestPhotoEntity, TestMongoPhoto>(entity, null);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Null(entity.PhotoId);
		_uowMock.Verify(u => u.AddAsync(entity), Times.Once);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task UpdateAsync_WithPhoto_UpdatesFileAndEntity()
	{
		// Arrange
		var entity = new TestPhotoEntity { PhotoId = "oldId" };  
		var existing = new TestPhotoEntity { PhotoId = "oldId" };
		var fileMock = new Mock<IFormFile>();
		var mongoPhoto = new TestMongoPhoto { Id = ObjectId.GenerateNewId() };

		_fileServiceMock
			.Setup(f => f.UpdateFileAsync<TestMongoPhoto>(fileMock.Object, existing.PhotoId, MediaTypeNames.Image.Jpeg))
			.ReturnsAsync(Result<TestMongoPhoto>.Success(mongoPhoto));

		// Act
		var result = await _repository.UpdateAsync<TestPhotoEntity, TestMongoPhoto>(entity, existing, fileMock.Object);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(mongoPhoto.Id.ToString(), existing.PhotoId);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task UpdateAsync_WithoutPhoto_UpdatesEntityOnly()
	{
		// Arrange
		var entity = new TestPhotoEntity { PhotoId = "oldId" };
		var existing = new TestPhotoEntity { PhotoId = "oldId" };

		// Act
		var result = await _repository.UpdateAsync<TestPhotoEntity, TestMongoPhoto>(entity, existing, null);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal("oldId", existing.PhotoId);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task DeleteAsync_DeletesFileAndEntity()
	{
		// Arrange
		var entity = new TestPhotoEntity { PhotoId = "123" };

		// Act
		await _repository.DeleteAsync<TestPhotoEntity, TestMongoPhoto>(entity);

		// Assert
		_fileServiceMock.Verify(f => f.DeleteFileAsync<TestMongoPhoto>("123"), Times.Once);
		_uowMock.Verify(u => u.Remove(entity), Times.Once);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}
}
