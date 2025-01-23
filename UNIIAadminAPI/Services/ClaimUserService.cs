using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbGenericRepository;
using System.Security.Claims;
using UNIIAadminAPI.Models;

namespace UNIIAadminAPI.Services
{
    public static class ClaimUserService
    {
        public static async Task<string?> GetAuthUserIdByUserId(ObjectId userId, IMongoDbContext db)
        {
            var adminUser = await db.GetCollection<AdminUser>()
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (adminUser != null)
            {
                return adminUser.AuthUserId.ToString();
            }

            var user = await db.GetCollection<User>()
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();

            return user?.AuthUserId.ToString();
        }

        public static async Task<byte[]> GetUserPictureFromClaims(IEnumerable<Claim> claims, HttpClient httpClient)
        {
            var pictureUrl = claims.FirstOrDefault(c => c.Type == "picture")!.Value;

            var response = await httpClient.GetAsync(pictureUrl);

            response.EnsureSuccessStatusCode();

            var imageBytesTask = await response.Content.ReadAsByteArrayAsync();

            return imageBytesTask;
        }
    }
}
