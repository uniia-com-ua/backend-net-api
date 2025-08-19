using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using MongoDB.Bson;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.FileServices;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using Xunit;

namespace UniiaAdmin.WebApi.Tests.ServiceTests;

public class FileEntityServiceTests
{
	private readonly Mock<IMongoUnitOfWork> _mongoMock;
	private readonly Mock<IFileProcessingService> _fileProcessingMock;
	private readonly Mock<IStringLocalizer<ErrorMessages>> _localizerMock;
	private readonly FileEntityService _service;

	public FileEntityServiceTests()
	{
		_mongoMock = new Mock<IMongoUnitOfWork>();
		_fileProcessingMock = new Mock<IFileProcessingService>();
		_localizerMock = new Mock<IStringLocalizer<ErrorMessages>>();

		_localizerMock.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
			.Returns((string key, object[] args) => new Microsoft.Extensions.Localization.LocalizedString(key, $"{key} {string.Join(", ", args)}"));

		_service = new FileEntityService(_fileProcessingMock.Object, _mongoMock.Object, _localizerMock.Object);
	}

	public class TestFileEntity : IMongoFileEntity
	{
		public ObjectId Id { get; set; }
		public byte[]? File { get; set; }
	}

	[Fact]
	public async Task GetFileAsync_NullOrEmptyId_ReturnsFailure()
	{
		var result = await _service.GetFileAsync<TestFileEntity>(null);
		Assert.False(result.IsSuccess);
		Assert.IsType<ArgumentException>(result.Error);
	}

	[Fact]
	public async Task GetFileAsync_InvalidObjectId_ReturnsFailure()
	{
		var result = await _service.GetFileAsync<TestFileEntity>("invalid");
		Assert.False(result.IsSuccess);
		Assert.IsType<InvalidDataException>(result.Error);
	}

	[Fact]
	public async Task GetFileAsync_FileNotFound_ReturnsFailure()
	{
		var id = ObjectId.GenerateNewId().ToString();
		_mongoMock.Setup(m => m.FindFileAsync<TestFileEntity>(It.IsAny<ObjectId>()))
			.ReturnsAsync((TestFileEntity?)null);

		var result = await _service.GetFileAsync<TestFileEntity>(id);
		Assert.False(result.IsSuccess);
		Assert.IsType<KeyNotFoundException>(result.Error);
	}

	[Fact]
	public async Task GetFileAsync_FileExists_ReturnsSuccess()
	{
		var id = ObjectId.GenerateNewId();
		var file = new TestFileEntity { Id = id, File = new byte[] { 1, 2, 3 } };
		_mongoMock.Setup(m => m.FindFileAsync<TestFileEntity>(id))
			.ReturnsAsync(file);

		var result = await _service.GetFileAsync<TestFileEntity>(id.ToString());
		Assert.True(result.IsSuccess);
		Assert.Equal(file, result.Value);
	}

	[Fact]
	public async Task SaveFileAsync_CallsDependencies_ReturnsSuccess()
	{
		var fileMock = new Mock<IFormFile>();
		var entity = new TestFileEntity { File = new byte[] { 1, 2 } };

		_fileProcessingMock.Setup(f => f.GetFileEntityAsync<TestFileEntity>(fileMock.Object, null))
			.ReturnsAsync(entity);
		_mongoMock.Setup(m => m.AddAsync(entity)).Returns(Task.CompletedTask);
		_mongoMock.Setup(m => m.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await _service.SaveFileAsync<TestFileEntity>(fileMock.Object, null);

		Assert.True(result.IsSuccess);
		Assert.Equal(entity, result.Value);
		_mongoMock.Verify(m => m.AddAsync(entity), Times.Once);
		_mongoMock.Verify(m => m.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task DeleteFileAsync_InvalidId_ReturnsFailure()
	{
		var result = await _service.DeleteFileAsync<TestFileEntity>("invalid");
		Assert.False(result.IsSuccess);
		Assert.IsType<ArgumentException>(result.Error);
	}

	[Fact]
	public async Task DeleteFileAsync_FileNotFound_ReturnsSuccessNoContent()
	{
		var id = ObjectId.GenerateNewId();
		_mongoMock.Setup(m => m.FindFileAsync<TestFileEntity>(id))
			.ReturnsAsync((TestFileEntity?)null);

		var result = await _service.DeleteFileAsync<TestFileEntity>(id.ToString());
		Assert.True(result.IsSuccess);
		Assert.Null(result.Value);
	}

	[Fact]
	public async Task DeleteFileAsync_FileExists_CallsRemoveAndSave()
	{
		var id = ObjectId.GenerateNewId();
		var entity = new TestFileEntity { Id = id, File = new byte[] { 1 } };

		_mongoMock.Setup(m => m.FindFileAsync<TestFileEntity>(id))
			.ReturnsAsync(entity);
		_mongoMock.Setup(m => m.Remove(entity));
		_mongoMock.Setup(m => m.SaveChangesAsync()).Returns(Task.CompletedTask);

		var result = await _service.DeleteFileAsync<TestFileEntity>(id.ToString());

		Assert.True(result.IsSuccess);
		_mongoMock.Verify(m => m.Remove(entity), Times.Once);
		_mongoMock.Verify(m => m.SaveChangesAsync(), Times.Once);
	}
}
