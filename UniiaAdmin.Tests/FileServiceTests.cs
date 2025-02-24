using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniiaAdmin.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using MongoDB.Bson;
    using MongoDB.Driver.Linq;
    using Moq;
    using NUnit.Framework;
    using UniiaAdmin.Data.Common;
    using UniiaAdmin.Data.Data;
    using UniiaAdmin.Data.Interfaces;
    using UniiaAdmin.Data.Interfaces.FileInterfaces;
    using UniiaAdmin.Data.Models;
    using UniiaAdmin.WebApi.FileServices;

    [TestFixture]
    public class FileEntityServiceTests
    {
        private Mock<IFileProcessingService> _fileProcessingServiceMock;
        private Mock<MongoDbContext> _mongoDbContextMock;
        private FileEntityService _fileEntityService;
        private Mock<DbSet<AuthorPhoto>> _dbSetMock;

        [SetUp]
        public void Setup()
        {
            _fileProcessingServiceMock = new Mock<IFileProcessingService>();
            _mongoDbContextMock = new Mock<MongoDbContext>();
            _dbSetMock = new Mock<DbSet<AuthorPhoto>>();

            _fileEntityService = new FileEntityService(_fileProcessingServiceMock.Object, _mongoDbContextMock.Object);
        }

        [TestCase(null)]
        [TestCase("")]
        public async Task GetFileAsync_InvalidFileId_ReturnsFailure(string fileId)
        {
            var result = await _fileEntityService.GetFileAsync(fileId, _dbSetMock.Object);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsInstanceOf<ArgumentException>(result.Error);
        }

        [Test]
        public async Task SaveFileAsync_ValidFile_ReturnsSuccess()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
            var testEntity = new AuthorPhoto { Id = ObjectId.GenerateNewId(), File = new byte[] { 1, 2, 3 } };

            _fileProcessingServiceMock.Setup(f => f.GetFileEntityAsync<AuthorPhoto>(fileMock.Object, "image/jpeg"))
                .ReturnsAsync(testEntity);

            _dbSetMock.Setup(d => d.AddAsync(testEntity, default)).ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuthorPhoto>)null);
            _mongoDbContextMock.Setup(m => m.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _fileEntityService.SaveFileAsync(fileMock.Object, _dbSetMock.Object, "image/jpeg");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(testEntity, result.Value);
        }
    }
}
