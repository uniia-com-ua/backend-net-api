namespace UniiaAdmin.WebApi.Tests.RepositoryTests;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using Moq;
using System.Net.Mime;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Repository;
using Xunit;

public class FileRepositoryTests
{
	private readonly Mock<IApplicationUnitOfWork> _uowMock;
	private readonly Mock<IFileEntityService> _fileServiceMock;
	private readonly Mock<IMapper> _mapperMock;
	private readonly FileRepository _repository;

	public FileRepositoryTests()
	{
		_uowMock = new Mock<IApplicationUnitOfWork>();
		_fileServiceMock = new Mock<IFileEntityService>();
		_mapperMock = new Mock<IMapper>();

		_repository = new FileRepository(_uowMock.Object, _mapperMock.Object, _fileServiceMock.Object);
	}

	private class TestFileEntity : IFileEntity
	{
		public int Id { get; set; }
		public string? FileId { get; set; }
	}

	private class TestMongoFileEntity : IMongoFileEntity
	{
		public ObjectId Id { get; set; }
		public byte[]? File { get; set; }
	}

	[Fact]
	public async Task CreateAsync_WithFile_Success()
	{
		// Arrange
		var fileEntity = new TestFileEntity();
		var formFile = new Mock<IFormFile>();
		var objectId = new ObjectId();

		_fileServiceMock.Setup(f => f.SaveFileAsync<TestMongoFileEntity>(formFile.Object, MediaTypeNames.Application.Pdf))
			.ReturnsAsync(Result<TestMongoFileEntity>.Success(new TestMongoFileEntity { Id = objectId }));

		_uowMock.Setup(u => u.AddAsync(fileEntity)).Returns(Task.CompletedTask);

		// Act
		var result = await _repository.CreateAsync<TestFileEntity, TestMongoFileEntity>(fileEntity, formFile.Object);

		// Assert
		Assert.True(result.IsSuccess);
		_uowMock.Verify(u => u.AddAsync(fileEntity), Times.Once);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task CreateAsync_SaveFileFails_ReturnsFailure()
	{
		// Arrange
		var fileEntity = new TestFileEntity();
		var formFile = new Mock<IFormFile>();

		_fileServiceMock.Setup(f => f.SaveFileAsync<TestMongoFileEntity>(formFile.Object, MediaTypeNames.Application.Pdf))
			.ReturnsAsync(Result<TestMongoFileEntity>.Failure(new Exception("error")));

		// Act
		var result = await _repository.CreateAsync<TestFileEntity, TestMongoFileEntity>(fileEntity, formFile.Object);

		// Assert
		Assert.False(result.IsSuccess);
		_uowMock.Verify(u => u.AddAsync(It.IsAny<TestFileEntity>()), Times.Never);
	}

	[Fact]
	public async Task UpdateAsync_WithFile_Success()
	{
		// Arrange
		var existing = new TestFileEntity { Id = 1, FileId = "old" };
		var updated = new TestFileEntity { Id = 1, FileId = "new" };
		var formFile = new Mock<IFormFile>();

		_fileServiceMock.Setup(f => f.UpdateFileAsync<TestMongoFileEntity>(formFile.Object, "new", MediaTypeNames.Application.Pdf))
			.ReturnsAsync(Result<TestMongoFileEntity>.Success(new TestMongoFileEntity { Id = new ObjectId() }));

		// Act
		var result = await _repository.UpdateAsync<TestFileEntity, TestMongoFileEntity>(updated, existing, formFile.Object);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(new ObjectId().ToString(), updated.FileId);
	}

	[Fact]
	public async Task UpdateAsync_NoFile_JustSaveChanges()
	{
		// Arrange
		var existing = new TestFileEntity { Id = 1, FileId = "old" };
		var updated = new TestFileEntity { Id = 1, FileId = "old" };

		// Act
		var result = await _repository.UpdateAsync<TestFileEntity, TestMongoFileEntity>(updated, existing, null);

		// Assert
		Assert.True(result.IsSuccess);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task DeleteAsync_DeletesFileAndEntity()
	{
		// Arrange
		var entity = new TestFileEntity { Id = 1, FileId = "abc" };

		_fileServiceMock.Setup(f => f.DeleteFileAsync<TestMongoFileEntity>("abc"))
			.ReturnsAsync(Result<TestMongoFileEntity>.SuccessNoContent());

		// Act
		await _repository.DeleteAsync<TestFileEntity, TestMongoFileEntity>(entity);

		// Assert
		_fileServiceMock.Verify(f => f.DeleteFileAsync<TestMongoFileEntity>("abc"), Times.Once);
		_uowMock.Verify(u => u.Remove(entity), Times.Once);
		_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
	}
}
