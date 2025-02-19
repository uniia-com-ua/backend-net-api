using Microsoft.Extensions.Configuration.UserSecrets;
using MongoDB.Bson;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;

namespace UniiaAdmin.WebApi.Services
{
    public class LogActionService
    {
        private readonly MongoDbContext _dbContext;
        public LogActionService(MongoDbContext mongoDbContext) 
        {
            _dbContext = mongoDbContext;
        }

        public async Task LogActionAsync<T>(AdminUser? adminUser, int modelId, string action) where T : class 
        {
            await _dbContext.LogActionModels.AddAsync(new()
            {
                Id = ObjectId.GenerateNewId(),
                UserId = adminUser!.Id,
                ModelId = modelId,
                ModelAction = action,
                ModelName = typeof(T).Name.ToLower(),
                ChangedTime = DateTime.Now,
            });

            await _dbContext.SaveChangesAsync();
        }
    }
}
