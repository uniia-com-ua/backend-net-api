using Microsoft.AspNetCore.Http;
using Moq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.FileServices;
using Xunit;


namespace UniiaAdmin.WebApi.Tests.ServiceTests;
public class FileProcessingServiceTests
{
	private readonly Mock<IFileValidationService> _validationMock;
	private readonly FileProcessingService _service;

	public FileProcessingServiceTests()
	{
		_validationMock = new Mock<IFileValidationService>();
		_service = new FileProcessingService(_validationMock.Object);
	}

	public class TestFileEntity : IMongoFileEntity
	{
		public ObjectId Id { get; set; }
		public byte[]? File { get; set; }
	}

	private IFormFile CreateMockFormFile(string content = "Hello World")
	{
		var bytes = Encoding.UTF8.GetBytes(content);
		var stream = new MemoryStream(bytes);
		return new FormFile(stream, 0, bytes.Length, "Data", "test.txt");
	}

	[Fact]
	public async Task GetFileEntityAsync_NewEntity_ValidatesFileAndSetsIdAndFile()
	{
		// Arrange
		var file = CreateMockFormFile();
		_validationMock.Setup(v => v.ValidateFile(file, null));

		// Act
		var result = await _service.GetFileEntityAsync<TestFileEntity>(file, null);

		// Assert
		_validationMock.Verify(v => v.ValidateFile(file, null), Times.Once);
		Assert.NotEqual(ObjectId.Empty, result.Id);
		Assert.NotNull(result.File);
		Assert.Equal(Encoding.UTF8.GetBytes("Hello World"), result.File);
	}

	[Fact]
	public async Task GetFileEntityAsync_ExistingEntity_UpdatesFile()
	{
		// Arrange
		var file = CreateMockFormFile("Updated Content");
		_validationMock.Setup(v => v.ValidateFile(file, "text/plain"));

		var existing = new TestFileEntity
		{
			Id = ObjectId.GenerateNewId(),
			File = Encoding.UTF8.GetBytes("Old Content")
		};

		// Act
		var result = await _service.GetFileEntityAsync(file, "text/plain", existing);

		// Assert
		_validationMock.Verify(v => v.ValidateFile(file, "text/plain"), Times.Once);
		Assert.Equal(existing.Id, result.Id); // Id не змінюється
		Assert.Equal(Encoding.UTF8.GetBytes("Updated Content"), result.File);
	}
}
