using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using UNIIAadminAPI.Enums;
using UNIIAadminAPI.Models;
using UNIIAadminAPI.Serializers;
using UNIIAadminAPI.Services;
using MongoDbGenericRepository;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("admin/api/subjects")]
    public class SubjectController(IMongoDbContext db) : ControllerBase
    {
        private readonly IMongoCollection<Subject> _subjectsCollection = db.GetCollection<Subject>();
        [HttpGet]
        [Route("get")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewSubject)]
        public async Task<IActionResult> Get(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var subject = await _subjectsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (subject == null)
                return NotFound();

            return Ok(new SubjectDto(subject));
        }

        [HttpGet]
        [Route("get-all")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewSubject)]
        public IActionResult GetAll()
        {
            List<SubjectDto> result = [];

            var subjects = _subjectsCollection.AsQueryable();

            foreach (var subject in subjects)
            {
                result.Add(new SubjectDto(subject));
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.CreateSubject)]
        public async Task<IActionResult> Create([FromBody] SubjectDto subjectDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Subject subject = new(subjectDto);

            await _subjectsCollection.InsertOneAsync(subject);

            return Ok();
        }

        [HttpPatch]
        [Route("update")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.UpdateSubject)]
        public async Task<IActionResult> Update([FromForm] SubjectDto subjectDto, string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectId objectId = ObjectId.Parse(id);

            var subject = await _subjectsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (subject == null)
                return NotFound();

            subject.UpdateByDtoModel(subjectDto);

            var filter = Builders<Subject>.Filter.Eq(a => a.Id, subject.Id);

            await _subjectsCollection.FindOneAndReplaceAsync(filter, subject);

            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.DeleteSubject)]
        public async Task<IActionResult> Delete(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var result = await _subjectsCollection.FindOneAndDeleteAsync(a => a.Id == objectId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
