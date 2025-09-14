using Microsoft.Extensions.Localization;
using Moq;
using System.IO;
using Microsoft.AspNetCore.Http;
using UniiaAdmin.WebApi.FileServices;
using UniiaAdmin.WebApi.Resources;
using Xunit;

namespace UniiaAdmin.WebApi.Tests.ServiceTests
{
	public class PdfFileValidatorTests
	{
		private readonly PdfFileValidator _validator;
		private readonly Mock<IStringLocalizer<ErrorMessages>> _localizerMock;

		public PdfFileValidatorTests()
		{
			_localizerMock = new Mock<IStringLocalizer<ErrorMessages>>();
			_localizerMock.Setup(l => l["OnlyPdfAllowed"]).Returns(new LocalizedString("OnlyPdfAllowed", "Only .pdf files are allowed"));

			var allowedExtensions = new[] { ".pdf" };
			_validator = new PdfFileValidator(allowedExtensions, _localizerMock.Object);
		}

		private static IFormFile CreateFormFile(string fileName)
		{
			var stream = new MemoryStream(new byte[] { 1, 2, 3 });
			return new FormFile(stream, 0, stream.Length, "file", fileName);
		}

		[Theory]
		[InlineData("document.pdf")]
		[InlineData("REPORT.PDF")]
		public void Validate_AllowedExtensions_DoesNotThrow(string fileName)
		{
			//Arrange
			var file = CreateFormFile(fileName);

			//Act
			var ex = Record.Exception(() => _validator.Validate(file));

			//Assert
			Assert.Null(ex);
		}

		[Theory]
		[InlineData("image.jpg")]
		[InlineData("file.txt")]
		[InlineData("presentation.pptx")]
		public void Validate_DisallowedExtensions_ThrowsInvalidDataException(string fileName)
		{
			//Arrange
			var file = CreateFormFile(fileName);

			//Act & Assert
			var ex = Assert.Throws<InvalidDataException>(() => _validator.Validate(file));
			Assert.Equal("Only .pdf files are allowed", ex.Message);
		}
	}
}
