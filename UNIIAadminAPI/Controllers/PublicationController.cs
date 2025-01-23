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
    [Route("admin/api/publications")]
    public class PublicationController(IMongoDbContext db, IMongoDatabase database) : ControllerBase
    {
        private readonly IMongoCollection<Publication> _publicationsCollection = db.GetCollection<Publication>();
        private readonly GridFSBucket _gridFS = new(database);

        [HttpGet]
        [Route("get")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewPublications)]
        public async Task<IActionResult> Get(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var publication = await _publicationsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (publication == null)
                return NotFound();

            return Ok(new PublicationOutDto(publication));
        }

        [HttpGet]
        [Route("get-file")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewPublications)]
        public async Task<IActionResult> GetFile(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var publication = await _publicationsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (publication == null)
                return NotFound();

            var fileStream = await _gridFS.OpenDownloadStreamAsync(publication.FileId);
            var fileInfo = fileStream.FileInfo;

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);

            var contentType = "application/pdf";

            return File(memoryStream.ToArray(), contentType);
        }

        [HttpGet]
        [Route("get-all")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewPublications)]
        public async Task<IActionResult> GetAll()
        {
            List<PublicationOutDto> result = [];

            var publications = _publicationsCollection.AsQueryable();

            foreach (var publication in publications)
            {
                var fileStream = await _gridFS.OpenDownloadStreamAsync(publication.FileId);
                
                var fileInfo = fileStream.FileInfo;

                using var memoryStream = new MemoryStream();

                await fileStream.CopyToAsync(memoryStream);

                result.Add(new PublicationOutDto(publication));
            }

            return Ok(result);
        }

/*        [HttpGet]
        [Route("get-all-files")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.ViewPublications)]
        public async Task<IActionResult> GetAll()
        {
            List<PublicationOutDto> result = [];

            var publications = _publicationsCollection.AsQueryable();

            foreach (var publication in publications)
            {
                var fileStream = await _gridFS.OpenDownloadStreamAsync(publication.FileId);

                var fileInfo = fileStream.FileInfo;

                using var memoryStream = new MemoryStream();

                await fileStream.CopyToAsync(memoryStream);

                result.Add(new PublicationOutDto(publication, memoryStream.ToArray()));
            }

            return Ok(result);
        }*/

        [HttpPost]
        [Route("create")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.CreatePublications)]
        public async Task<IActionResult> Create([FromForm] PublicationDto publicationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectId fileId = ObjectId.Empty;

            if (publicationDto.File != null && publicationDto.File.Length > 0)
            {
                using var stream = publicationDto.File.OpenReadStream();
                fileId = await _gridFS.UploadFromStreamAsync(publicationDto.File.FileName, stream);
            }

            Publication publication = new(publicationDto, fileId);

            await _publicationsCollection.InsertOneAsync(publication);

            return Ok();
        }

        [HttpPatch]
        [Route("update")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.UpdatePublications)]
        public async Task<IActionResult> Update([FromForm] PublicationDto publicationDto, string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectId objectId = ObjectId.Parse(id);

            var publication = await _publicationsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (publication == null)
                return NotFound();

            publication.UpdateByDtoModel(publicationDto);

            var filter = Builders<Publication>.Filter.Eq(a => a.Id, publication.Id);

            await _publicationsCollection.FindOneAndReplaceAsync(filter, publication);

            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        [ValidateToken]
        [RequireClaim(ClaimsEnum.DeletePublications)]
        public async Task<IActionResult> Delete(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var author = await _publicationsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

            if (author == null)
            {
                return NotFound();
            }

            _gridFS.Delete(author.FileId);

            await _publicationsCollection.FindOneAndDeleteAsync(a => a.Id == objectId);

            return Ok();
        }
    }
}
