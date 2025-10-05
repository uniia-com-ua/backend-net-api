using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Controllers;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using Xunit;

namespace UniiaAdmin.WebApi.Tests.ControllerTests
{
	public class FacultyControllerTests
	{
		private readonly ControllerWebAppFactory<FacultyController> _factory;
		private readonly JsonSerializerOptions _jsonOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public FacultyControllerTests()
		{
			var mockProvider = new MockProvider();

			mockProvider.Mock<IGenericRepository>();
			mockProvider.Mock<IApplicationUnitOfWork>();
			mockProvider.Mock<MongoDbContext>();

			var localizer = mockProvider.Mock<IStringLocalizer<ErrorMessages>>();
			localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
				.Returns((string key, object[] args) => new LocalizedString(key, $"{key} {string.Join(", ", args)}"));

			_factory = new ControllerWebAppFactory<FacultyController>(mockProvider);
		}

		[Fact]
		public async Task GetFaculty_InvalidId_Returns404()
		{
			// Arrange
			const int invalidId = 999;
			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.FindAsync<Faculty>(invalidId))
				.ReturnsAsync((Faculty)null!);

			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync($"/api/v1/faculties/{invalidId}");

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public async Task GetFaculty_ValidId_Returns200AndFaculty()
		{
			// Arrange
			const int validId = 1;
			var faculty = new Faculty { Id = validId, FullName = "CS Faculty", UniversityId = 10 };

			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.FindAsync<Faculty>(validId))
				.ReturnsAsync(faculty);

			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync($"/api/v1/faculties/{validId}");
			var returned = await DeserializeResponse<Faculty>(response);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Equal(faculty.FullName, returned?.FullName);
		}

		[Fact]
		public async Task GetPagedFaculties_Returns200WithList()
		{
			// Arrange
			var faculties = new PageData<Faculty>
			{
				Items = new() { new Faculty { Id = 1, FullName = "Math Faculty", UniversityId = 5 } },
				TotalCount = 1
			};

			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.GetPagedAsync<Faculty>(0, 10, null))
				.ReturnsAsync(faculties);

			var client = _factory.CreateClient();

			// Act
			var response = await client.GetAsync("/api/v1/faculties/page");
			var returned = await DeserializeResponse<PageData<Faculty>>(response);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			Assert.Single(returned!.Items);
		}

		[Fact]
		public async Task CreateFaculty_UniversityNotFound_Returns404()
		{
			// Arrange
			var faculty = new Faculty { Id = 1, FullName = "New Faculty", ShortName = "new fac", UniversityId = 99 };

			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.AnyAsync<University>(faculty.UniversityId))
				.ReturnsAsync(false);

			var client = _factory.CreateClient();
			var content = new StringContent(JsonSerializer.Serialize(faculty), Encoding.UTF8, "application/json");

			// Act
			var response = await client.PostAsync("/api/v1/faculties", content);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public async Task CreateFaculty_Success_Returns200()
		{
			// Arrange
			var faculty = new Faculty { Id = 1, FullName = "New Faculty", ShortName = "new fac", UniversityId = 10 };

			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.AnyAsync<University>(faculty.UniversityId))
				.ReturnsAsync(true);

			_factory.Mocks.Mock<IGenericRepository>()
				.Setup(r => r.CreateAsync(faculty))
				.Returns(Task.CompletedTask);

			var client = _factory.CreateClient();
			var content = new StringContent(JsonSerializer.Serialize(faculty), Encoding.UTF8, "application/json");

			// Act
			var response = await client.PostAsync("/api/v1/faculties", content);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		[Fact]
		public async Task UpdateFaculty_NotFound_Returns404()
		{
			// Arrange
			const int id = 1;
			var faculty = new Faculty { Id = id, FullName = "Update Faculty", ShortName = "new fac", UniversityId = 10 };

			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.FindAsync<Faculty>(id))
				.ReturnsAsync((Faculty)null!);

			var client = _factory.CreateClient();
			var content = new StringContent(JsonSerializer.Serialize(faculty), Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync($"/api/v1/faculties/{id}", content);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public async Task UpdateFaculty_UniversityNotFound_Returns404()
		{
			// Arrange
			const int id = 1;
			var faculty = new Faculty { Id = id, FullName = "Update Faculty", ShortName = "new fac", UniversityId = 99 };
			var existingFaculty = new Faculty { Id = id, FullName = "Old Faculty", ShortName = "old fac", UniversityId = 1 };

			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.FindAsync<Faculty>(id))
				.ReturnsAsync(existingFaculty);

			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.AnyAsync<University>(faculty.UniversityId))
				.ReturnsAsync(false);

			var client = _factory.CreateClient();
			var content = new StringContent(JsonSerializer.Serialize(faculty), Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync($"/api/v1/faculties/{id}", content);

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public async Task UpdateFaculty_Success_Returns200()
		{
			// Arrange
			const int id = 1;
			var faculty = new Faculty { Id = id, FullName = "Update Faculty", ShortName = "new fac", UniversityId = 10 };
			var existingFaculty = new Faculty { Id = id, FullName = "Old Faculty", ShortName = "old fac", UniversityId = 1 };

			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.FindAsync<Faculty>(id))
				.ReturnsAsync(existingFaculty);

			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.AnyAsync<University>(faculty.UniversityId))
				.ReturnsAsync(true);

			_factory.Mocks.Mock<IGenericRepository>()
				.Setup(r => r.UpdateAsync(faculty, existingFaculty))
				.Returns(Task.CompletedTask);

			var client = _factory.CreateClient();
			var content = new StringContent(JsonSerializer.Serialize(faculty), Encoding.UTF8, "application/json");

			// Act
			var response = await client.PatchAsync($"/api/v1/faculties/{id}", content);

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		[Fact]
		public async Task DeleteFaculty_NotFound_Returns404()
		{
			// Arrange
			const int id = 1;
			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.FindAsync<Faculty>(id))
				.ReturnsAsync((Faculty)null!);

			var client = _factory.CreateClient();

			// Act
			var response = await client.DeleteAsync($"/api/v1/faculties/{id}");

			// Assert
			Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
		}

		[Fact]
		public async Task DeleteFaculty_Success_Returns200()
		{
			// Arrange
			const int id = 1;
			var faculty = new Faculty { Id = id, FullName = "Delete Faculty", UniversityId = 10 };

			_factory.Mocks.Mock<IApplicationUnitOfWork>()
				.Setup(r => r.FindAsync<Faculty>(id))
				.ReturnsAsync(faculty);

			_factory.Mocks.Mock<IGenericRepository>()
				.Setup(r => r.DeleteAsync(faculty))
				.Returns(Task.CompletedTask);

			var client = _factory.CreateClient();

			// Act
			var response = await client.DeleteAsync($"/api/v1/faculties/{id}");

			// Assert
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		}

		private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
		{
			var json = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<T>(json, _jsonOptions);
		}
	}
}
