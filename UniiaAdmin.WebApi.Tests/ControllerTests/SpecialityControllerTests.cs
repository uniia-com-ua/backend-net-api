namespace UniiaAdmin.WebApi.Tests.ControllerTests;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Localization;
using Moq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UniiaAdmin.Data.Common;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Controllers;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using Xunit;

public class SpecialityControllerTests
{
	private readonly ControllerWebAppFactory<SpecialityController> _factory;

	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public SpecialityControllerTests()
	{
		var mockProvider = new MockProvider();

		mockProvider.Mock<IGenericRepository>();
		mockProvider.Mock<IApplicationUnitOfWork>();
		mockProvider.Mock<MongoDbContext>();

		var localizer = mockProvider.Mock<IStringLocalizer<ErrorMessages>>();

		localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
			.Returns((string key, object[] args) => new LocalizedString(key, $"{key} {string.Join(", ", args)}"));

		_factory = new ControllerWebAppFactory<SpecialityController>(mockProvider);
	}

	[Fact]
	public async Task GetSpeciality_InvalidId_Returns404()
	{
		// Arrange
		const int invalidId = 999;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Specialty>(invalidId))
			.ReturnsAsync((Specialty)null!);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/specialities/{invalidId}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetSpeciality_ValidId_Returns200AndSpeciality()
	{
		// Arrange
		const int validId = 1;
		var speciality = new Specialty { Id = validId, Name = "Psychology" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Specialty>(validId))
			.ReturnsAsync(speciality);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync($"/api/v1/specialities/{validId}");
		var returned = await DeserializeResponse<Specialty>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(speciality.Name, returned?.Name);
	}

	[Fact]
	public async Task GetPagedSpecialities_Returns200WithList()
	{
		// Arrange
		var specialities = new List<Specialty>
		{
			new Specialty { Id = 1, Name = "Psychology" }
		};

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.GetPagedAsync<Specialty>(0, 10))
			.ReturnsAsync(specialities);

		var client = _factory.CreateClient();

		// Act
		var response = await client.GetAsync("/api/v1/specialities/page");
		var returned = await DeserializeResponse<List<Specialty>>(response);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!);
	}

	[Fact]
	public async Task CreateSpeciality_Success_Returns200()
	{
		// Arrange
		var client = _factory.CreateClient();
		var speciality = new Specialty { Id = 1, Name = "Psychology" };
		var content = new MultipartFormDataContent { { new StringContent(speciality.Name), "Name" } };

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.CreateAsync(It.IsAny<Specialty>()))
			.Returns(Task.CompletedTask);

		// Act
		var response = await client.PostAsync("/api/v1/specialities", content);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task UpdateSpeciality_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Specialty>(id))
			.ReturnsAsync((Specialty)null!);

		var client = _factory.CreateClient();
		var content = new MultipartFormDataContent { { new StringContent("NewName"), "Name" } };

		// Act
		var response = await client.PatchAsync($"/api/v1/specialities/{id}", content);

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task UpdateSpeciality_Success_Returns200()
	{
		// Arrange
		const int id = 1;
		var oldSpeciality = new Specialty { Id = id, Name = "OldName" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Specialty>(id))
			.ReturnsAsync(oldSpeciality);

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.UpdateAsync(It.IsAny<Specialty>(), oldSpeciality))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();
		var content = new MultipartFormDataContent { { new StringContent("NewName"), "Name" } };

		// Act
		var response = await client.PatchAsync($"/api/v1/specialities/{id}", content);

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task DeleteSpeciality_NotFound_Returns404()
	{
		// Arrange
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Specialty>(id))
			.ReturnsAsync((Specialty)null!);

		var client = _factory.CreateClient();

		// Act
		var response = await client.DeleteAsync($"/api/v1/specialities/{id}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task DeleteSpeciality_Success_Returns200()
	{
		// Arrange
		const int id = 1;
		var speciality = new Specialty { Id = id, Name = "Psychology" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Specialty>(id))
			.ReturnsAsync(speciality);

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.DeleteAsync(speciality))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();

		// Act
		var response = await client.DeleteAsync($"/api/v1/specialities/{id}");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	private async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
	{
		var json = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<T>(json, _jsonOptions);
	}
}
