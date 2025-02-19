using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniiaAdmin.Data.Interfaces.FileInterfaces
{
    public interface IFileValidator
    {
        void Validate(IFormFile file);
    }
}
