namespace UniiaAdmin.Tests.ControllerTests;

using Microsoft.Extensions.Localization;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Controllers;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Resources;
using Xunit;

public class SubjectControllerTests
{
	private readonly ControllerWebAppFactory<SubjectController> _factory;

	public SubjectControllerTests()
	{
		var mockProvider = new MockProvider();

		mockProvider.Mock<IGenericRepository>();
		mockProvider.Mock<IApplicationUnitOfWork>();
		mockProvider.Mock<MongoDbContext>();

		var localizer = mockProvider.Mock<IStringLocalizer<ErrorMessages>>();
		localizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
			.Returns((string key, object[] args) =>
				new LocalizedString(key, $"{key} {string.Join(", ", args)}"));

		_factory = new ControllerWebAppFactory<SubjectController>(mockProvider);
	}

	[Fact]
	public async Task GetSubject_InvalidId_Returns404()
	{
		const int invalidId = 999;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Subject>(invalidId))
			.ReturnsAsync((Subject)null!);

		var client = _factory.CreateClient();

		var response = await client.GetAsync($"/api/v1/subjects/{invalidId}");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task GetSubject_ValidId_Returns200AndSubject()
	{
		const int validId = 1;
		var subject = new Subject { Id = validId, Name = "Math" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Subject>(validId))
			.ReturnsAsync(subject);

		var client = _factory.CreateClient();

		var response = await client.GetAsync($"/api/v1/subjects/{validId}");
		var returned = await DeserializeResponse<Subject>(response);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(subject.Name, returned?.Name);
	}

	[Fact]
	public async Task GetPagedSubjects_Returns200WithList()
	{
		var subjects = new List<Subject>
		{
			new Subject { Id = 1, Name = "Math" }
		};

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.GetPagedAsync<Subject>(0, 10))
			.ReturnsAsync(subjects);

		var client = _factory.CreateClient();

		var response = await client.GetAsync("/api/v1/subjects/page");
		var returned = await DeserializeResponse<List<Subject>>(response);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Single(returned!);
	}

	[Fact]
	public async Task CreateSubject_Success_Returns200()
	{
		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("Math"), Encoding.UTF8, "application/json");

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.CreateAsync(It.IsAny<Subject>()))
			.Returns(Task.CompletedTask);

		var response = await client.PostAsync("/api/v1/subjects", content);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task UpdateSubject_NotFound_Returns404()
	{
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Subject>(id))
			.ReturnsAsync((Subject)null!);

		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("NewName"), Encoding.UTF8, "application/json");

		var response = await client.PatchAsync($"/api/v1/subjects/{id}", content);

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task UpdateSubject_Success_Returns200()
	{
		const int id = 1;
		var oldSubject = new Subject { Id = id, Name = "OldName" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Subject>(id))
			.ReturnsAsync(oldSubject);

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.UpdateAsync(It.IsAny<Subject>(), oldSubject))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();
		var content = new StringContent(JsonSerializer.Serialize("NewName"), Encoding.UTF8, "application/json");

		var response = await client.PatchAsync($"/api/v1/subjects/{id}", content);

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	[Fact]
	public async Task DeleteSubject_NotFound_Returns404()
	{
		const int id = 1;
		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Subject>(id))
			.ReturnsAsync((Subject)null!);

		var client = _factory.CreateClient();

		var response = await client.DeleteAsync($"/api/v1/subjects/{id}");

		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	[Fact]
	public async Task DeleteSubject_Success_Returns200()
	{
		const int id = 1;
		var subject = new Subject { Id = id, Name = "Math" };

		_factory.Mocks.Mock<IApplicationUnitOfWork>()
			.Setup(r => r.FindAsync<Subject>(id))
			.ReturnsAsync(subject);

		_factory.Mocks.Mock<IGenericRepository>()
			.Setup(r => r.DeleteAsync(subject))
			.Returns(Task.CompletedTask);

		var client = _factory.CreateClient();

		var response = await client.DeleteAsync($"/api/v1/subjects/{id}");

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	private static async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
	{
		var json = await response.Content.ReadAsStringAsync();
		return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		});
	}
}
