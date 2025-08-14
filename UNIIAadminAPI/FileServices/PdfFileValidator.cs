using Microsoft.Extensions.Localization;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Resources;

namespace UniiaAdmin.WebApi.FileServices
{
    public class PdfFileValidator : IFileValidator
    {
		private string[] _allowedFileExtensions;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public PdfFileValidator(
            string[] allowedFileExtentions,
			 IStringLocalizer<ErrorMessages> localizer)
		{
			_allowedFileExtensions = allowedFileExtentions;
            _localizer = localizer;
		}

		public void Validate(IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!_allowedFileExtensions.Contains(fileExtension))
            {
                throw new InvalidDataException(_localizer["OnlyPdfAllowed"].Value);
            }
        }
    }
}
