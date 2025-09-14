using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniiaAdmin.WebApi.Interfaces.FileInterfaces
{
    public interface IFileValidationService
    {
        void ValidateFile(IFormFile file, string? mediaType);
    }
}
