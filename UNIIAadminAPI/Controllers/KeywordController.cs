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
    [Route("admin/api/keywords")]
    public class KeywordController(IMongoDbContext db) : ControllerBase
    {
        private readonly IMongoCollection<Keyword> _keywordsCollection = db.GetCollection<Keyword>();

        [HttpGet]
        [Route("get")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewKeyword)]
        public async Task<IActionResult> Get(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var keyword = await _keywordsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (keyword == null)
                return NotFound();

            return Ok(new KeywordDto(keyword));
        }

        [HttpGet]
        [Route("get-all")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewKeyword)]
        public IActionResult GetAll()
        {
            List<KeywordDto> result = [];

            var keywords = _keywordsCollection.AsQueryable();

            foreach (var keyword in keywords)
            {
                result.Add(new KeywordDto(keyword));
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.CreateKeyword)]
        public async Task<IActionResult> Create([FromBody] KeywordDto keywordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Keyword keyword = new(keywordDto);

            await _keywordsCollection.InsertOneAsync(keyword);

            return Ok();
        }

        [HttpPatch]
        [Route("update")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.UpdateKeyword)]
        public async Task<IActionResult> Update([FromBody] KeywordDto keywordDto, string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectId objectId = ObjectId.Parse(id);

            var keyword = await _keywordsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (keyword == null)
                return NotFound();

            keyword.UpdateByDtoModel(keywordDto);

            var filter = Builders<Keyword>.Filter.Eq(a => a.Id, keyword.Id);

            await _keywordsCollection.FindOneAndReplaceAsync(filter, keyword);

            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.DeleteKeyword)]
        public async Task<IActionResult> Delete(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var result = await _keywordsCollection.FindOneAndDeleteAsync(a => a.Id == objectId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
