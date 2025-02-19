using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Sprache;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Interfaces.FileInterfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Constants;

namespace UniiaAdmin.WebApi.FileServices
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly IFileValidationService _fileValidationService;
        public FileProcessingService(IFileValidationService fileValidationService)
        {
            _fileValidationService = fileValidationService;
        }
        public async Task<T> GetFileEntityAsync<T>(IFormFile file, string? mediaType)
            where T : class, IMongoFileEntity, new()
        {
            _fileValidationService.ValidateFile(file, mediaType);

            T entity = new()
            {
                Id = ObjectId.GenerateNewId(),
                File = await ConvertToByteArrayAsync(file)
            };

            return entity;
        }

        public async Task<T> GetFileEntityAsync<T>(IFormFile file, string? mediaType, T entity)
            where T : class, IMongoFileEntity
        {
            _fileValidationService.ValidateFile(file, mediaType);

            entity.File = await ConvertToByteArrayAsync(file);

            return entity;
        }

        private static async Task<byte[]> ConvertToByteArrayAsync(IFormFile photoFile)
        {
            using var memoryStream = new MemoryStream();

            await photoFile.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }
    }
}
