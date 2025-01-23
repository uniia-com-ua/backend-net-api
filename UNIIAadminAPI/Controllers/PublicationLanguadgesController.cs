using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbGenericRepository;
using UNIIAadminAPI.Enums;
using UNIIAadminAPI.Models;
using UNIIAadminAPI.Serializers;
using UNIIAadminAPI.Services;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("admin/api/publicationLanguadges")]
    public class PublicationLanguadgesController(IMongoDbContext db) : ControllerBase
    {
        private readonly IMongoCollection<PublicationLanguage> _publicationLanguadgesCollection = db.GetCollection<PublicationLanguage>();

        [HttpGet]
        [Route("get")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewPublicationLanguadges)]
        public async Task<IActionResult> Get(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var publicationLanguage = await _publicationLanguadgesCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (publicationLanguage == null)
                return NotFound();

            return Ok(new PublicationLanguageDto(publicationLanguage));
        }

        [HttpGet]
        [Route("get-all")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewPublicationLanguadges)]
        public IActionResult GetAll()
        {
            List<PublicationLanguageDto> result = [];

            var publicationLanguages = _publicationLanguadgesCollection.AsQueryable();

            foreach (var publicationLanguage in publicationLanguages)
            {
                result.Add(new PublicationLanguageDto(publicationLanguage));
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.CreatePublicationLanguadges)]
        public async Task<IActionResult> Create([FromBody] PublicationLanguageDto publicationLanguageDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PublicationLanguage publicationLanguage = new(publicationLanguageDto);

            await _publicationLanguadgesCollection.InsertOneAsync(publicationLanguage);

            return Ok();
        }

        [HttpPatch]
        [Route("update")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.UpdatePublicationLanguadges)]
        public async Task<IActionResult> Update([FromForm] PublicationLanguageDto publicationLanguageDto, string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectId objectId = ObjectId.Parse(id);

            var publicationLanguage = await _publicationLanguadgesCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (publicationLanguage == null)
                return NotFound();

            publicationLanguage.UpdateByDtoModel(publicationLanguageDto);

            var filter = Builders<PublicationLanguage>.Filter.Eq(a => a.Id, publicationLanguage.Id);

            await _publicationLanguadgesCollection.FindOneAndReplaceAsync(filter, publicationLanguage);

            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.DeletePublicationLanguadges)]
        public async Task<IActionResult> Delete(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var result = await _publicationLanguadgesCollection.FindOneAndDeleteAsync(a => a.Id == objectId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
