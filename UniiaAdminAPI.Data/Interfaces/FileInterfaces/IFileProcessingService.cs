using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniiaAdmin.Data.Interfaces.FileInterfaces
{
    public interface IFileProcessingService
    {
        Task<T> GetFileEntityAsync<T>(IFormFile file, string? mediaType) where T : class, IMongoFileEntity, new();
        Task<T> GetFileEntityAsync<T>(IFormFile file, string? mediaType, T entity) where T : class, IMongoFileEntity;
    }
}
