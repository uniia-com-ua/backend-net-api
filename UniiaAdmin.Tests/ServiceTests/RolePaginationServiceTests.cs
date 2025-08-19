using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UniiaAdmin.WebApi.Services;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.WebApi.Interfaces;
using Xunit;

namespace UniiaAdmin.Tests.ServiceTests;
public class RolePaginationServiceTests
{
	private readonly Mock<IAdminUnitOfWork> _adminMock;
	private readonly Mock<IPaginationService> _paginationMock;
	private readonly Mock<IRoleRepository> _roleRepoMock;
	private readonly RolePaginationService _service;

	public RolePaginationServiceTests()
	{
		_adminMock = new Mock<IAdminUnitOfWork>();
		_paginationMock = new Mock<IPaginationService>();
		_roleRepoMock = new Mock<IRoleRepository>();

		_service = new RolePaginationService(_adminMock.Object, _paginationMock.Object, _roleRepoMock.Object);
	}

	[Fact]
	public async Task GetPagedClaimsAsync_AllRoles_CallsPaginationService()
	{
		// Arrange
		const int skip = 0;
		const int take = 10;

		var roleClaims = new List<IdentityRoleClaim<string>>
		{
			new() { RoleId = "1", ClaimValue = "ClaimA" },
			new() { RoleId = "2", ClaimValue = "ClaimB" },
		}.AsQueryable();

		_adminMock.Setup(a => a.RoleClaims()).Returns(roleClaims);
		_paginationMock.Setup(p => p.GetPagedListAsync(It.IsAny<IQueryable<string>>(), skip, take))
					   .ReturnsAsync(roleClaims.Select(rc => rc.ClaimValue!).Distinct().OrderBy(c => c).ToList());

		// Act
		var result = await _service.GetPagedClaimsAsync(skip, take);

		// Assert
		Assert.Equal(2, result!.Count);
		Assert.Contains("ClaimA", result);
		Assert.Contains("ClaimB", result);
		_paginationMock.Verify(p => p.GetPagedListAsync(It.IsAny<IQueryable<string>>(), skip, take), Times.Once);
	}

	[Fact]
	public async Task GetPagedClaimsAsync_ByRoleId_CallsPaginationService()
	{
		// Arrange
		const int skip = 0;
		const int take = 5;
		const string roleId = "1";

		var roleClaims = new List<IdentityRoleClaim<string>>
		{
			new() { RoleId = roleId, ClaimValue = "ClaimA" },
			new() { RoleId = "2", ClaimValue = "ClaimB" },
		}.AsQueryable();

		_adminMock.Setup(a => a.RoleClaims()).Returns(roleClaims);
		_paginationMock.Setup(p => p.GetPagedListAsync(It.IsAny<IQueryable<string>>(), skip, take))
					   .ReturnsAsync(roleClaims.Where(rc => rc.RoleId == roleId).Select(rc => rc.ClaimValue!).Distinct().OrderBy(c => c).ToList());

		// Act
		var result = await _service.GetPagedClaimsAsync(roleId, skip, take);

		// Assert
		Assert.Single(result!);
		Assert.Equal("ClaimA", result![0]);
		_paginationMock.Verify(p => p.GetPagedListAsync(It.IsAny<IQueryable<string>>(), skip, take), Times.Once);
	}

	[Fact]
	public async Task GetPagedRolesAsync_CallsPaginationService()
	{
		// Arrange
		const int skip = 0;
		const int take = 5;

		var roles = new List<IdentityRole>
		{
			new() { Name = "Admin" },
			new() { Name = "User" }
		}.AsQueryable();

		_roleRepoMock.Setup(r => r.Roles()).Returns(roles);
		_paginationMock.Setup(p => p.GetPagedListAsync(roles, skip, take))
					   .ReturnsAsync(roles.ToList());

		// Act
		var result = await _service.GetPagedRolesAsync(skip, take);

		// Assert
		Assert.Equal(2, result!.Count);
		Assert.Contains(result, r => r.Name == "Admin");
		Assert.Contains(result, r => r.Name == "User");
		_paginationMock.Verify(p => p.GetPagedListAsync(roles, skip, take), Times.Once);
	}
}
