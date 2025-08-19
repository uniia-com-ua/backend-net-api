using Microsoft.Extensions.Localization;
using Moq;
using System.IO;
using Microsoft.AspNetCore.Http;
using UniiaAdmin.WebApi.FileServices;
using UniiaAdmin.WebApi.Resources;
using Xunit;

namespace UniiaAdmin.WebApi.Tests.ServiceTests
{
	public class JpegFileValidatorTests
	{
		private readonly JpegFileValidator _validator;
		private readonly Mock<IStringLocalizer<ErrorMessages>> _localizerMock;

		public JpegFileValidatorTests()
		{
			_localizerMock = new Mock<IStringLocalizer<ErrorMessages>>();
			_localizerMock.Setup(l => l["OnlyJpgAllowed"]).Returns(new LocalizedString("OnlyJpgAllowed", "Only .jpg files are allowed"));

			var allowedExtensions = new[] { ".jpg", ".jpeg" };
			_validator = new JpegFileValidator(allowedExtensions, _localizerMock.Object);
		}

		private static IFormFile CreateFormFile(string fileName)
		{
			var stream = new MemoryStream(new byte[] { 1, 2, 3 });
			return new FormFile(stream, 0, stream.Length, "file", fileName);
		}

		[Theory]
		[InlineData("test.jpg")]
		[InlineData("photo.jpeg")]
		[InlineData("IMAGE.JPEG")]
		public void Validate_AllowedExtensions_DoesNotThrow(string fileName)
		{
			// Arrange
			var file = CreateFormFile(fileName);

			// Act & Assert
			var ex = Record.Exception(() => _validator.Validate(file));
			Assert.Null(ex);
		}

		[Theory]
		[InlineData("test.png")]
		[InlineData("document.pdf")]
		[InlineData("image.bmp")]
		public void Validate_DisallowedExtensions_ThrowsInvalidDataException(string fileName)
		{
			// Arrange
			var file = CreateFormFile(fileName);

			// Act & Assert
			var ex = Assert.Throws<InvalidDataException>(() => _validator.Validate(file));
			Assert.Equal("Only .jpg files are allowed", ex.Message);
		}
	}
}
