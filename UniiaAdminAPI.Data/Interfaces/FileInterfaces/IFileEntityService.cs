using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UniiaAdmin.Data.Common;

namespace UniiaAdmin.Data.Interfaces.FileInterfaces
{
    public interface IFileEntityService
    {
        Task<Result<T>> GetFileAsync<T>(string? fileId, IQueryable<T> dbSet) where T : class, IMongoFileEntity;
        Task<Result<T>> SaveFileAsync<T>(IFormFile file, DbSet<T> dbSet, string? mediaType) where T : class, IMongoFileEntity, new();
        Task<Result<T>> UpdateFileAsync<T>(IFormFile file, string? fileId, DbSet<T> dbSet, string? mediaType) where T : class, IMongoFileEntity, new();
        Task<Result<T>> DeleteFileAsync<T>(string? fileId, DbSet<T> dbSet) where T : class, IMongoFileEntity;
    }
}
