using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDbGenericRepository;
using UNIIAadminAPI.Enums;
using UNIIAadminAPI.Models;
using UNIIAadminAPI.Serializers;
using UNIIAadminAPI.Services;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("admin/api/authors")]
    public class AuthorController(IMongoDbContext db, IMongoDatabase database) : ControllerBase
    {
        private readonly IMongoCollection<Author> _authorsCollection = db.GetCollection<Author>();
        private readonly GridFSBucket _gridFS = new(database);

        [HttpGet]
        [Route("get")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewAuthors)]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("The 'id' parameter is required and cannot be empty");
            }

            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return BadRequest("Invalid 'id' format");
            }

            var author = await _authorsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if(author == null)
                return NotFound("Author with id '{id}' not found");

            return Ok(new AuthorOutDto(author));
        }

        [HttpGet]
        [Route("get-picture")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewAuthors)]
        public async Task<IActionResult> GetPicture(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var author = await _authorsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (author == null)
                return NotFound();

            var fileStream = await _gridFS.OpenDownloadStreamAsync(author.PhotoId);

            var fileInfo = fileStream.FileInfo;

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);

            string contentType = "image/jpeg";

            return File(memoryStream.ToArray(), contentType);
        }

        [HttpGet]
        [Route("get-all")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewAuthors)]
        public IActionResult GetAll()
        {
            List<AuthorOutDto> result = [];

            var authors = _authorsCollection.AsQueryable();

            foreach(var author in authors)
            {
                result.Add(new AuthorOutDto(author));
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.CreateAuthors)]
        public async Task<IActionResult> Create([FromForm] AuthorDto authorDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectId fileId = ObjectId.Empty;

            if (authorDto.Photo_file != null && authorDto.Photo_file.Length > 0)
            {
                using var stream = authorDto.Photo_file.OpenReadStream();
                fileId = await _gridFS.UploadFromStreamAsync(authorDto.Photo_file.FileName, stream);
            }

            Author author = new(authorDto, fileId);

            await _authorsCollection.InsertOneAsync(author);

            return Ok();
        }

        [HttpPatch]
        [Route("update")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.UpdateAuthors)]
        public async Task<IActionResult> Update([FromForm] AuthorDto authorDto, string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectId objectId = ObjectId.Parse(id);

            var author = await _authorsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (author == null)
                return NotFound();

            author.UpdateByDtoModel(authorDto);

            var filter = Builders<Author>.Filter.Eq(a => a.Id, author.Id); 

            await _authorsCollection.FindOneAndReplaceAsync(filter, author);

            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.DeleteAuthors)]
        public async Task<IActionResult> Delete(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var author = await _authorsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if(author == null)
            {
                return NotFound();
            }

            _gridFS.Delete(author.PhotoId);

            await _authorsCollection.FindOneAndDeleteAsync(a => a.Id == objectId);

            return Ok();
        }
    }
}
