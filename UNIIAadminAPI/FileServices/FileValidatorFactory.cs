using System.Net.Mime;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.WebApi.FileServices
{
    public class FileValidatorFactory : IFileValidatorFactory
    {
        public IFileValidator? GetValidator(string? mediaType)
        {
            return mediaType switch
            {
                MediaTypeNames.Image.Jpeg => new JpegFileValidator(),
                MediaTypeNames.Application.Pdf => new PdfFileValidator(),
                _ => null
            };
        }
    }
}
