using Microsoft.Extensions.Localization;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Resources;

namespace UniiaAdmin.WebApi.FileServices
{
    public class JpegFileValidator : IFileValidator
    {
		private string[] _allowedImageExtensions;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public JpegFileValidator(
            string[] allowedImageExtensions,
			IStringLocalizer<ErrorMessages> localizer) 
        {
            _allowedImageExtensions = allowedImageExtensions;
            _localizer = localizer;
        }

        public void Validate(IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!_allowedImageExtensions.Contains(fileExtension))
            {
                throw new InvalidDataException(_localizer["OnlyJpgAllowed"].Value);
            }
        }
    }
}
