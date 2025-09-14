using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Interfaces.FileInterfaces;

namespace UniiaAdmin.WebApi.Interfaces.FileInterfaces
{
    public interface IFileEntityService
    {
        Task<Result<T>> GetFileAsync<T>(string? fileId) where T : class, IMongoFileEntity;
        Task<Result<T>> SaveFileAsync<T>(IFormFile file, string? mediaType) where T : class, IMongoFileEntity, new();
        Task<Result<T>> UpdateFileAsync<T>(IFormFile file, string? fileId, string? mediaType) where T : class, IMongoFileEntity, new();
        Task<Result<T>> DeleteFileAsync<T>(string? fileId) where T : class, IMongoFileEntity;
    }
}
