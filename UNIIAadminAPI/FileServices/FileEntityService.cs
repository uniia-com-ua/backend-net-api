using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MongoDB.Bson;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Resources;

namespace UniiaAdmin.WebApi.FileServices
{
    public class FileEntityService : IFileEntityService
    {
        private readonly IFileProcessingService _fileProcessingService;
        private readonly MongoDbContext _mongoDbContext;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public FileEntityService(
            IFileProcessingService fileProcessingService,
            MongoDbContext mongoDbContext,
			IStringLocalizer<ErrorMessages> localizer) 
        {
            _fileProcessingService = fileProcessingService;
            _mongoDbContext = mongoDbContext;
            _localizer = localizer;
        }

        public async Task<Result<T>> GetFileAsync<T>(string? fileId, DbSet<T> dbSet)
            where T : class, IMongoFileEntity
        {
            try
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    return Result<T>.Failure(new ArgumentException(_localizer["ModelFileIdIsNull", typeof(T).Name].Value));
                }

                if (!ObjectId.TryParse(fileId, out var objectId))
                {
                    return Result<T>.Failure(new InvalidDataException(_localizer["FileParsingFailed", fileId].Value));
                }

                var file = await dbSet.FindAsync(objectId);

				if (file?.File == null)
                {
                    return Result<T>.Failure(new KeyNotFoundException(_localizer["ModelFileWithIdNotFound", typeof(T).Name, fileId].Value));
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
                    return Result<T>.Failure(new ArgumentException(_localizer["FileParsingFailed", fileId].Value));
                }

                var isExist = await dbSet.AnyAsync(a => a.Id == objectId);

                if (!isExist)
				{
                    return await SaveFileAsync(file, dbSet, mediaType);
                }

				var photo = new T { Id = objectId };

                dbSet.Attach(photo);

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
                    return Result<T>.Failure(new ArgumentException(_localizer["FileParsingFailed", fileId!].Value));
                }

                var file = await dbSet.FindAsync(objectId);

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
