namespace UniiaAdmin.WebApi.Tests.RepositoryTests;

using MongoDB.Bson;
using Moq;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Repository;
using Xunit;

public class PhotoProviderTests
{
	private readonly Mock<IApplicationUnitOfWork> _uowMock;
	private readonly Mock<IFileEntityService> _fileServiceMock;
	private readonly PhotoProvider _provider;

	public PhotoProviderTests()
	{
		_uowMock = new Mock<IApplicationUnitOfWork>();
		_fileServiceMock = new Mock<IFileEntityService>();
		_provider = new PhotoProvider(_uowMock.Object, _fileServiceMock.Object);
	}

	private class TestPhotoEntity : IPhotoEntity
	{
		public int Id { get; set; }
		public string? PhotoId { get; set; }
	}

	private class TestMongoPhoto : IMongoFileEntity
	{
		public ObjectId Id { get; set; }
		public byte[]? File { get; set; }
	}

	[Fact]
	public async Task GetPhotoAsync_ReturnsPhotoFromFileService()
	{
		// Arrange
		const int entityId = 1;
		const string photoId = "someObjectId";
		var mongoPhoto = new TestMongoPhoto { Id = ObjectId.GenerateNewId(), File = [ 1, 2, 3 ] };

		_uowMock.Setup(u => u.FindPhotoIdAsync<TestPhotoEntity>(It.IsAny<int>()))
				.ReturnsAsync(photoId);

		_fileServiceMock.Setup(f => f.GetFileAsync<TestMongoPhoto>(photoId))
						.ReturnsAsync(Result<TestMongoPhoto>.Success(mongoPhoto));

		// Act
		var result = await _provider.GetPhotoAsync<TestPhotoEntity, TestMongoPhoto>(entityId);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(mongoPhoto, result.Value);
		_fileServiceMock.Verify(f => f.GetFileAsync<TestMongoPhoto>(photoId), Times.Once);
	}

	[Fact]
	public async Task GetPhotoAsync_ReturnsFailure_WhenPhotoIdIsNull()
	{
		// Arrange
		const int entityId = 1;

		_uowMock.Setup(u => u.FindPhotoIdAsync<TestPhotoEntity>(entityId))
				.ReturnsAsync((string?)null);

		// Act
		var result = await _provider.GetPhotoAsync<TestPhotoEntity, TestMongoPhoto>(entityId);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.IsType<KeyNotFoundException>(result.Error);
		_fileServiceMock.Verify(f => f.GetFileAsync<TestMongoPhoto>(It.IsAny<string>()), Times.Never);
	}

	[Fact]
	public async Task GetPhotoAsync_ReturnsFailure_WhenFileServiceFails()
	{
		// Arrange
		const int entityId = 1;
		var photoId = "someObjectId";

		_uowMock.Setup(u => u.FindPhotoIdAsync<TestPhotoEntity>(entityId))
				.ReturnsAsync(photoId);

		_fileServiceMock.Setup(f => f.GetFileAsync<TestMongoPhoto>(photoId))
						.ReturnsAsync(Result<TestMongoPhoto>.Failure(new KeyNotFoundException()));

		// Act
		var result = await _provider.GetPhotoAsync<TestPhotoEntity, TestMongoPhoto>(entityId);

		// Assert
		Assert.False(result.IsSuccess);
		_fileServiceMock.Verify(f => f.GetFileAsync<TestMongoPhoto>(photoId), Times.Once);
	}
}
