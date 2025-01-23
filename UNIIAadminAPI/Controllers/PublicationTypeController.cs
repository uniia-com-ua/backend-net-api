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
    [Route("admin/api/publicationTypes")]
    public class PublicationTypeController(IMongoDbContext db) : ControllerBase
    {
        private readonly IMongoCollection<PublicationType> _publicationTypesCollection = db.GetCollection<PublicationType>();

        [HttpGet]
        [Route("get")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewPublicationTypes)]
        public async Task<IActionResult> Get(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var publicationType = await _publicationTypesCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (publicationType == null)
                return NotFound();

            return Ok(new PublicationTypeDto(publicationType));
        }

        [HttpGet]
        [Route("get-all")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewPublicationTypes)]
        public IActionResult GetAll()
        {
            List<PublicationTypeDto> result = [];

            var publicationTypes = _publicationTypesCollection.AsQueryable();

            foreach (var publicationType in publicationTypes)
            {
                result.Add(new PublicationTypeDto(publicationType));
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.CreatePublicationTypes)]
        public async Task<IActionResult> Create([FromBody] PublicationTypeDto publicationTypeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PublicationType publicationType = new(publicationTypeDto);

            await _publicationTypesCollection.InsertOneAsync(publicationType);

            return Ok();
        }

        [HttpPatch]
        [Route("update")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.UpdatePublicationTypes)]
        public async Task<IActionResult> Update([FromBody] PublicationTypeDto publicationTypeDto, string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectId objectId = ObjectId.Parse(id);

            var publicationType = await _publicationTypesCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (publicationType == null)
                return NotFound();

            publicationType.UpdateByDtoModel(publicationTypeDto);

            var filter = Builders<PublicationType>.Filter.Eq(a => a.Id, publicationType.Id);

            await _publicationTypesCollection.FindOneAndReplaceAsync(filter, publicationType);

            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.DeletePublicationTypes)]
        public async Task<IActionResult> Delete(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var result = await _publicationTypesCollection.FindOneAndDeleteAsync(a => a.Id == objectId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
