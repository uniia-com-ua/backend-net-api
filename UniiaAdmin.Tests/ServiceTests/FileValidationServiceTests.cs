using Microsoft.AspNetCore.Http;
using Moq;
using System.IO;
using System.Text;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.FileServices;
using Xunit;

public class FileValidationServiceTests
{
	private readonly Mock<IFileValidatorFactory> _factoryMock;
	private readonly FileValidationService _service;

	public FileValidationServiceTests()
	{
		_factoryMock = new Mock<IFileValidatorFactory>();
		_service = new FileValidationService(_factoryMock.Object);
	}

	private IFormFile CreateMockFormFile(string content = "Hello World")
	{
		var bytes = Encoding.UTF8.GetBytes(content);
		var stream = new MemoryStream(bytes);
		return new FormFile(stream, 0, bytes.Length, "Data", "test.txt");
	}

	[Fact]
	public void ValidateFile_CallsValidator_WhenValidatorExists()
	{
		// Arrange
		var file = CreateMockFormFile();
		var mediaType = "image/jpeg";

		var validatorMock = new Mock<IFileValidator>();
		_factoryMock.Setup(f => f.GetValidator(mediaType)).Returns(validatorMock.Object);

		// Act
		_service.ValidateFile(file, mediaType);

		// Assert
		_factoryMock.Verify(f => f.GetValidator(mediaType), Times.Once);
		validatorMock.Verify(v => v.Validate(file), Times.Once);
	}

	[Fact]
	public void ValidateFile_DoesNothing_WhenValidatorIsNull()
	{
		// Arrange
		var file = CreateMockFormFile();
		var mediaType = "unknown/type";

		_factoryMock.Setup(f => f.GetValidator(mediaType)).Returns((IFileValidator?)null);

		// Act
		_service.ValidateFile(file, mediaType);

		// Assert
		_factoryMock.Verify(f => f.GetValidator(mediaType), Times.Once);
		// Тут нічого робити не має, тому перевірка на те, що метод Validate не викликається
	}
}
