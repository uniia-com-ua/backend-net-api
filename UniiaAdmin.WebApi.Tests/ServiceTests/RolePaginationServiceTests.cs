using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UniiaAdmin.WebApi.Services;
using UniiaAdmin.WebApi.Interfaces;
using Xunit;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.Data.Dtos;

namespace UniiaAdmin.WebApi.Tests.ServiceTests;
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

		var roleClaims = new PageData<IdentityRoleClaim<string>>
		{
			Items = new List<IdentityRoleClaim<string>>() 
			{
				new() { RoleId = "1", ClaimValue = "ClaimA" },
				new() { RoleId = "2", ClaimValue = "ClaimB" } 
			},
			TotalCount = 2
		};

		_adminMock.Setup(a => a.RoleClaims()).Returns(roleClaims.Items.AsQueryable());
		_paginationMock.Setup(p => p.GetPagedListAsync(It.IsAny<IQueryable<string>>(), skip, take, null))
			   .ReturnsAsync(() =>
			   {
				   var claims = roleClaims.Items.Select(rc => rc.ClaimValue!).Distinct().OrderBy(c => c).ToList();
				   return new PageData<string> { Items = claims, TotalCount = claims.Count };
			   });

		// Act
		var result = await _service.GetPagedClaimsAsync(skip, take);

		// Assert
		Assert.Equal(2, result!.Items.Count);
		Assert.Contains("ClaimA", result.Items);
		Assert.Contains("ClaimB", result.Items);
		_paginationMock.Verify(p => p.GetPagedListAsync(It.IsAny<IQueryable<string>>(), skip, take, null), Times.Once);
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
		_paginationMock.Setup(p => p.GetPagedListAsync(It.IsAny<IQueryable<string>>(), skip, take, null))
					   .ReturnsAsync(() =>
					   {
							var claims = roleClaims.Where(rc => rc.RoleId == roleId).Select(rc => rc.ClaimValue!).Distinct().OrderBy(c => c).ToList();
							return new PageData<string> { Items = claims, TotalCount = claims.Count };
				       });


		// Act
		var result = await _service.GetPagedClaimsAsync(roleId, skip, take);

		// Assert
		Assert.Single(result!.Items);
		Assert.Equal("ClaimA", result!.Items[0]);
		_paginationMock.Verify(p => p.GetPagedListAsync(It.IsAny<IQueryable<string>>(), skip, take, null), Times.Once);
	}

	[Fact]
	public async Task GetPagedRolesAsync_CallsPaginationService()
	{
		// Arrange
		const int skip = 0;
		const int take = 5;

		var roles = new PageData<IdentityRole>
		{
			Items = new() { new() { Name = "Admin" }, new() { Name = "User" } },
			TotalCount = 2
		};

		_roleRepoMock.Setup(r => r.Roles()).Returns(roles.Items.AsQueryable());
		_paginationMock.Setup(p => p.GetPagedListAsync(roles.Items.AsQueryable(), skip, take, null))
					   .ReturnsAsync(roles);

		// Act
		var result = await _service.GetPagedRolesAsync(skip, take);

		// Assert
		Assert.Equal(2, result!.Items.Count);
		Assert.Contains(result.Items, r => r.Name == "Admin");
		Assert.Contains(result.Items, r => r.Name == "User");
		_paginationMock.Verify(p => p.GetPagedListAsync(roles.Items.AsQueryable(), skip, take, null), Times.Once);
	}
}
