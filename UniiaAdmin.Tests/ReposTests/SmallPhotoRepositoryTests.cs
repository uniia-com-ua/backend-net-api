namespace UniiaAdmin.Tests.RepositoryTests
{
	using AutoMapper;
	using Microsoft.AspNetCore.Http;
	using Moq;
	using MongoDB.Bson;
	using System.Net.Mime;
	using System.Threading.Tasks;
	using UniiaAdmin.Data.Common;
	using UniiaAdmin.Data.Interfaces.FileInterfaces;
	using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
	using UniiaAdmin.WebApi.Repository;
	using Xunit;

	public class SmallPhotoRepositoryTests
	{
		private readonly Mock<IApplicationUnitOfWork> _uowMock;
		private readonly Mock<IFileEntityService> _fileServiceMock;
		private readonly Mock<IMapper> _mapperMock;
		private readonly SmallPhotoRepository _repository;

		public SmallPhotoRepositoryTests()
		{
			_uowMock = new Mock<IApplicationUnitOfWork>();
			_fileServiceMock = new Mock<IFileEntityService>();
			_mapperMock = new Mock<IMapper>();
			_repository = new SmallPhotoRepository(_uowMock.Object, _mapperMock.Object, _fileServiceMock.Object);
		}

		private class TestSmallPhotoEntity : ISmallPhotoEntity
		{
			public int Id { get; set; }
			public string? PhotoId { get; set; }
			public string? SmallPhotoId { get; set; }
		}

		private class TestMongoPhoto : IMongoFileEntity
		{
			public ObjectId Id { get; set; }
			public byte[]? File { get; set; }
		}

		[Fact]
		public async Task GetSmallPhotoAsync_ReturnsFile()
		{
			// Arrange
			const int entityId = 1;
			var photoId = "photo123";
			var mongoPhoto = new TestMongoPhoto { Id = ObjectId.GenerateNewId(), File = new byte[] { 1, 2, 3 } };

			_uowMock.Setup(u => u.FindSmallPhotoIdAsync<TestSmallPhotoEntity>(entityId))
					.ReturnsAsync(photoId);

			_fileServiceMock.Setup(f => f.GetFileAsync<TestMongoPhoto>(photoId))
							.ReturnsAsync(Result<TestMongoPhoto>.Success(mongoPhoto));

			// Act
			var result = await _repository.GetSmallPhotoAsync<TestSmallPhotoEntity, TestMongoPhoto>(entityId);

			// Assert
			Assert.True(result.IsSuccess);
			Assert.Equal(mongoPhoto, result.Value);
		}

		[Fact]
		public async Task CreateAsync_WithPhotos_SavesFilesAndEntity()
		{
			// Arrange
			var entity = new TestSmallPhotoEntity();
			var photoFileMock = new Mock<IFormFile>();
			var smallPhotoFileMock = new Mock<IFormFile>();
			var mongoPhoto1 = new TestMongoPhoto { Id = ObjectId.GenerateNewId() };
			var mongoPhoto2 = new TestMongoPhoto { Id = ObjectId.GenerateNewId() };

			_fileServiceMock.Setup(f => f.SaveFileAsync<TestMongoPhoto>(photoFileMock.Object, MediaTypeNames.Image.Jpeg))
							.ReturnsAsync(Result<TestMongoPhoto>.Success(mongoPhoto1));

			_fileServiceMock.Setup(f => f.SaveFileAsync<TestMongoPhoto>(smallPhotoFileMock.Object, MediaTypeNames.Image.Jpeg))
							.ReturnsAsync(Result<TestMongoPhoto>.Success(mongoPhoto2));

			// Act
			var result = await _repository.CreateAsync<TestSmallPhotoEntity, TestMongoPhoto>(entity, photoFileMock.Object, smallPhotoFileMock.Object);

			// Assert
			Assert.True(result.IsSuccess);
			Assert.Equal(mongoPhoto1.Id.ToString(), entity.PhotoId);
			Assert.Equal(mongoPhoto2.Id.ToString(), entity.SmallPhotoId);
			_uowMock.Verify(u => u.AddAsync(entity), Times.Once);
			_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
		}

		[Fact]
		public async Task UpdateAsync_WithPhotos_UpdatesFilesAndEntity()
		{
			// Arrange
			var entity = new TestSmallPhotoEntity { PhotoId = "oldPhoto", SmallPhotoId = "oldSmall" };
			var existing = new TestSmallPhotoEntity { PhotoId = "oldPhoto", SmallPhotoId = "oldSmall" };
			var photoFileMock = new Mock<IFormFile>();
			var smallPhotoFileMock = new Mock<IFormFile>();
			var mongoPhoto1 = new TestMongoPhoto { Id = ObjectId.GenerateNewId() };
			var mongoPhoto2 = new TestMongoPhoto { Id = ObjectId.GenerateNewId() };

			_fileServiceMock.Setup(f => f.UpdateFileAsync<TestMongoPhoto>(photoFileMock.Object, existing.PhotoId, MediaTypeNames.Image.Jpeg))
							.ReturnsAsync(Result<TestMongoPhoto>.Success(mongoPhoto1));

			_fileServiceMock.Setup(f => f.UpdateFileAsync<TestMongoPhoto>(smallPhotoFileMock.Object, existing.SmallPhotoId, MediaTypeNames.Image.Jpeg))
							.ReturnsAsync(Result<TestMongoPhoto>.Success(mongoPhoto2));

			// Act
			var result = await _repository.UpdateAsync<TestSmallPhotoEntity, TestMongoPhoto>(entity, existing, photoFileMock.Object, smallPhotoFileMock.Object);

			// Assert
			Assert.True(result.IsSuccess);
			Assert.Equal(mongoPhoto1.Id.ToString(), entity.PhotoId);
			Assert.Equal(mongoPhoto2.Id.ToString(), entity.SmallPhotoId);
			_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
		}

		[Fact]
		public async Task DeleteAsync_DeletesFilesAndEntity()
		{
			// Arrange
			var entity = new TestSmallPhotoEntity { PhotoId = "photoId", SmallPhotoId = "smallId" };

			// Act
			await _repository.DeleteAsync<TestSmallPhotoEntity, TestMongoPhoto>(entity);

			// Assert
			_fileServiceMock.Verify(f => f.DeleteFileAsync<TestMongoPhoto>(entity.PhotoId), Times.Once);
			_fileServiceMock.Verify(f => f.DeleteFileAsync<TestMongoPhoto>(entity.SmallPhotoId), Times.Once);
			_uowMock.Verify(u => u.Remove(entity), Times.Once);
			_uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
		}
	}
}
