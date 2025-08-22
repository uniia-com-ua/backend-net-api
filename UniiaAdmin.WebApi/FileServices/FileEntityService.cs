using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MongoDB.Bson;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.FileInterfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using UniiaAdmin.WebApi.Services;

namespace UniiaAdmin.WebApi.FileServices
{
    public class FileEntityService : IFileEntityService
    {
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IMongoUnitOfWork _mongoUnitOfWork;
		private readonly IStringLocalizer<ErrorMessages> _localizer;

		public FileEntityService(
            IFileProcessingService fileProcessingService,
			IMongoUnitOfWork mongoUnitOfWork,
			IStringLocalizer<ErrorMessages> localizer) 
        {
            _fileProcessingService = fileProcessingService;
			_mongoUnitOfWork = mongoUnitOfWork;
            _localizer = localizer;
        }

        public async Task<Result<T>> GetFileAsync<T>(string? fileId)
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

                var file = await _mongoUnitOfWork.FindFileAsync<T>(objectId);

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

        public async Task<Result<T>> SaveFileAsync<T>(IFormFile file, string? mediaType)
            where T : class, IMongoFileEntity, new()
        {
            try
            {
                var entity = await _fileProcessingService.GetFileEntityAsync<T>(file, mediaType);
                
                await _mongoUnitOfWork.AddAsync(entity);

                await _mongoUnitOfWork.SaveChangesAsync();

                return Result<T>.Success(entity);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex);
            }
        }

        public async Task<Result<T>> UpdateFileAsync<T>(IFormFile file, string? fileId, string? mediaType)
            where T : class, IMongoFileEntity, new()
        {
            try
            {
                if (string.IsNullOrEmpty(fileId))
                {
                    return await SaveFileAsync<T>(file, mediaType);
                }

                if (!ObjectId.TryParse(fileId, out var objectId))
                {
                    return Result<T>.Failure(new ArgumentException(_localizer["FileParsingFailed", fileId].Value));
                }

                var isExist = await _mongoUnitOfWork.AnyAsync<T>(objectId);

                if (!isExist)
				{
                    return await SaveFileAsync<T>(file, mediaType);
                }

				var photo = new T { Id = objectId };

                _mongoUnitOfWork.Attach(photo);

				photo = await _fileProcessingService.GetFileEntityAsync(file, mediaType, photo);

                _mongoUnitOfWork.Update(photo);

                await _mongoUnitOfWork.SaveChangesAsync();

                return Result<T>.Success(photo);
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex);
            }
        }

        public async Task<Result<T>> DeleteFileAsync<T>(string? fileId)
           where T : class, IMongoFileEntity
        {
            try
            {
                if (!ObjectId.TryParse(fileId, out var objectId))
                {
                    return Result<T>.Failure(new ArgumentException(_localizer["FileParsingFailed", fileId!].Value));
                }

                var file = await _mongoUnitOfWork.FindFileAsync<T>(objectId);

                if (file == null)
                {
                    return Result<T>.SuccessNoContent();
                }

                _mongoUnitOfWork.Remove(file);

                await _mongoUnitOfWork.SaveChangesAsync();

                return Result<T>.SuccessNoContent();
            }
            catch (Exception ex)
            {
                return Result<T>.Failure(ex);
            }
        }
    }
}
