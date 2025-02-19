using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Constants;

namespace UniiaAdmin.WebApi.FileServices
{
    public class JpegFileValidator : IFileValidator
    {
        public void Validate(IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!AppConstants.AllowedImageExtensions.Contains(fileExtension))
            {
                throw new InvalidDataException(ErrorMessages.OnlyJpgAllowed);
            }
        }
    }
}
