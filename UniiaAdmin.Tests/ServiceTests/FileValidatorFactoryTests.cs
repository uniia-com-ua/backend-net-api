using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Net.Mime;
using Moq;
using UniiaAdmin.WebApi.FileServices;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Resources;
using Xunit;

namespace UniiaAdmin.Tests.ServiceTests
{
	public class FileValidatorFactoryTests
	{
		private readonly FileValidatorFactory _factory;
		private readonly Mock<IStringLocalizer<ErrorMessages>> _localizerMock;

		public FileValidatorFactoryTests()
		{
			_localizerMock = new Mock<IStringLocalizer<ErrorMessages>>();

			var inMemorySettings = new Dictionary<string, string?>
			{
				{"FileSettings:AllowedImageExtensions:0", ".jpeg"},
				{"FileSettings:AllowedImageExtensions:1", ".jpg"},
				{"FileSettings:AllowedFileExtensions:0", ".pdf"}
			};
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(inMemorySettings)
				.Build();

			_factory = new FileValidatorFactory(configuration, _localizerMock.Object);
		}

		[Fact]
		public void GetValidator_ReturnsJpegValidator_ForJpegMediaType()
		{
			// Act
			var validator = _factory.GetValidator(MediaTypeNames.Image.Jpeg);

			// Assert
			Assert.NotNull(validator);
			Assert.IsType<JpegFileValidator>(validator);
		}

		[Fact]
		public void GetValidator_ReturnsPdfValidator_ForPdfMediaType()
		{
			// Act
			var validator = _factory.GetValidator(MediaTypeNames.Application.Pdf);

			// Assert
			Assert.NotNull(validator);
			Assert.IsType<PdfFileValidator>(validator);
		}

		[Fact]
		public void GetValidator_ReturnsNull_ForUnknownMediaType()
		{
			// Act
			var validator = _factory.GetValidator("unknown/type");

			// Assert
			Assert.Null(validator);
		}

		[Fact]
		public void GetValidator_ReturnsNull_ForNullMediaType()
		{
			// Act
			var validator = _factory.GetValidator(null);

			// Assert
			Assert.Null(validator);
		}
	}
}
