using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Constants;

namespace UniiaAdmin.WebApi.FileServices
{
    public class FileEntityService : IFileEntityService
    {
        private readonly IFileProcessingService _fileProcessingService;
        private readonly MongoDbContext _mongoDbContext;

        public FileEntityService(IFileProcessingService fileProcessingService,
            MongoDbContext mongoDbContext) 
        {
            _fileProcessingService = fileProcessingService;
            _mongoDbContext = mongoDbContext;
        }

        public async Task<Result<T>> GetFileAsync<T>(string? fileId, IQueryable<T> dbSet)
            where T : class, IMongoFileEntity
        {
            try
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    return Result<T>.Failure(new ArgumentException(ErrorMessages.ModelFileIdIsNull(typeof(T).Name)));
                }

                if (!ObjectId.TryParse(fileId, out var objectId))
                {
                    return Result<T>.Failure(new InvalidDataException(ErrorMessages.FileParsingFailed(fileId)));
                }

                var file = await dbSet.FirstOrDefaultAsync(a => a.Id == objectId);

                if (file?.File == null)
                {
                    return Result<T>.Failure(new KeyNotFoundException(ErrorMessages.ModelFileWithIdNotFound(typeof(T).Name, fileId)));
                }

                return Result<T>.Success(file);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex);
            }
        }

        public async Task<Result<T>> SaveFileAsync<T>(IFormFile file, DbSet<T> dbSet, string? mediaType)
            where T : class, IMongoFileEntity, new()
        {
            try
            {
                var entity = await _fileProcessingService.GetFileEntityAsync<T>(file, mediaType);
                
                await dbSet.AddAsync(entity);

                await _mongoDbContext.SaveChangesAsync();

                return Result<T>.Success(entity);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex);
            }
        }

        public async Task<Result<T>> UpdateFileAsync<T>(IFormFile file, string? fileId, DbSet<T> dbSet, string? mediaType)
            where T : class, IMongoFileEntity, new()
        {
            try
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    return await SaveFileAsync(file, dbSet, mediaType);
                }

                if (!ObjectId.TryParse(fileId, out var objectId))
                {
                    return Result<T>.Failure(new ArgumentException(ErrorMessages.FileParsingFailed(fileId)));
                }

                var photo = await dbSet.FirstOrDefaultAsync(a => a.Id == objectId);

                if (photo == null)
                {
                    return await SaveFileAsync(file, dbSet, mediaType);
                }

                photo = await _fileProcessingService.GetFileEntityAsync(file, mediaType, photo);

                dbSet.Update(photo);

                await _mongoDbContext.SaveChangesAsync();

                return Result<T>.Success(photo);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex);
            }
        }

        public async Task<Result<T>> DeleteFileAsync<T>(string? fileId, DbSet<T> dbSet)
           where T : class, IMongoFileEntity
        {
            try
            {
                if (!ObjectId.TryParse(fileId, out var objectId))
                {
                    return Result<T>.Failure(new ArgumentException(ErrorMessages.FileParsingFailed(fileId)));
                }

                var file = await dbSet.FirstOrDefaultAsync(af => af.Id == objectId);

                if (file == null)
                {
                    return Result<T>.SuccessNoContent();
                }

                dbSet.Remove(file);

                await _mongoDbContext.SaveChangesAsync();

                return Result<T>.SuccessNoContent();
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex);
            }
        }
    }
}
