using Microsoft.Extensions.Localization;
using System.Net.Mime;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Resources;

namespace UniiaAdmin.WebApi.FileServices
{
    public class FileValidatorFactory : IFileValidatorFactory
    {
        private readonly string[] _allowedImageExtensions;
		private readonly string[] _allowedFileExtensions;
        private readonly IStringLocalizer<ErrorMessages> _localizer;

		public FileValidatorFactory(
            IConfiguration configuration,
            IStringLocalizer<ErrorMessages> localizer) 
        {
			_allowedImageExtensions = configuration.GetSection("FileSettings:AllowedImageExtensions").Get<string[]>()!;
			_allowedFileExtensions= configuration.GetSection("FileSettings:AllowedFileExtensions").Get<string[]>()!;
            _localizer = localizer;
		}

        public IFileValidator? GetValidator(string? mediaType)
        {
            return mediaType switch
            {
                MediaTypeNames.Image.Jpeg => new JpegFileValidator(_allowedImageExtensions, _localizer),
                MediaTypeNames.Application.Pdf => new PdfFileValidator(_allowedFileExtensions, _localizer),
                _ => null
            };
        }
    }
}
