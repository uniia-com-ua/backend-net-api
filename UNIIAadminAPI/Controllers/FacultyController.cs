using Microsoft.AspNetCore.Http;
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
    [Route("admin/api/faculty")]
    public class FacultyController(IMongoDbContext db) : ControllerBase
    {
        private readonly IMongoCollection<Faculty> _facultyCollection = db.GetCollection<Faculty>();

        [HttpGet]
        [Route("get")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewFaculties)]
        public async Task<IActionResult> Get(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var faculty = await _facultyCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (faculty == null)
                return NotFound();

            return Ok(new FacultyDto(faculty));
        }

        [HttpGet]
        [Route("get-all")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewFaculties)]
        public IActionResult GetAll()
        {
            List<FacultyDto> result = [];

            var faculties = _facultyCollection.AsQueryable();

            foreach (var faculty in faculties)
            {
                result.Add(new FacultyDto(faculty));
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.CreateFaculties)]
        public async Task<IActionResult> Create([FromBody] FacultyDto facultyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Faculty faculty = new(facultyDto);

            await _facultyCollection.InsertOneAsync(faculty);

            return Ok();
        }

        [HttpPatch]
        [Route("update")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.UpdateFaculties)]
        public async Task<IActionResult> Update([FromForm] FacultyDto facultyDto, string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectId objectId = ObjectId.Parse(id);

            var faculty = await _facultyCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (faculty == null)
                return NotFound();

            faculty.UpdateByDtoModel(facultyDto);

            var filter = Builders<Faculty>.Filter.Eq(a => a.Id, faculty.Id);

            await _facultyCollection.FindOneAndReplaceAsync(filter, faculty);

            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.DeleteFaculties)]
        public async Task<IActionResult> Delete(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var result = await _facultyCollection.FindOneAndDeleteAsync(a => a.Id == objectId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}