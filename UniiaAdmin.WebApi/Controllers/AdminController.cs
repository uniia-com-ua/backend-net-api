using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Dtos.UserDtos;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;

namespace UniiaAdmin.WebApi.Controllers
{
    [Route("api/v1/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminUnitOfWork _adminUnitOfWork;
		public AdminController(IAdminUnitOfWork adminUnitOfWork)
        {
			_adminUnitOfWork = adminUnitOfWork;
        }

		/*        [HttpPatch]
				[Route("add-claim-to-user")]

				public async Task<IActionResult> AddAdminUserClaim(ClaimsEnum claimsEnum, string userId)
				{
					ObjectId objectId = new(userId);

					var authUserId = await ClaimUserService.GetAuthUserIdByUserId(objectId, db);

					if (string.IsNullOrEmpty(authUserId))
					{
						return BadRequest();
					}

					var relatedUser = await userManager.FindByIdAsync(authUserId);

					Claim claim = new("http://schemas.microsoft.com/ws/2008/06/identity/claims/user", claimsEnum.ToString());

					var result = await userManager.AddClaimAsync(relatedUser, claim);

					if (!result.Succeeded)
						return BadRequest();

					return Ok();
				}

				[HttpPatch]
				[Route("remove-claim-from-user")]

				[Authorize(Roles = "Admin")]
				public async Task<IActionResult> RemoveAdminUserClaim(ClaimsEnum claimsEnum, string userId)
				{
					ObjectId objectId = new(userId);

					var authUserId = await ClaimUserService.GetAuthUserIdByUserId(objectId, db);

					if (string.IsNullOrEmpty(authUserId))
						return NotFound();

					Claim claim = new("http://schemas.microsoft.com/ws/2008/06/identity/claims/user", claimsEnum.ToString());

					var relatedUser = await userManager.FindByIdAsync(authUserId);

					var result = await userManager.RemoveClaimAsync(relatedUser, claim);

					if (!result.Succeeded)
						return NotFound();

					return Ok();
				}*/

		[HttpGet("page")]
		[Permission(PermissionResource.AdminUsers, CrudActions.View)]
		public async Task<IActionResult> GetAllUsers()
        {
			var users = await _adminUnitOfWork.GetListAsync();

			return Ok(users);
        }

        [HttpGet("{id}")]
		[Permission(PermissionResource.AdminUsers, CrudActions.View)]
		public async Task<IActionResult> GetUserById(string id)
        {
            AdminUserDto? user = await _adminUnitOfWork.GetAsync(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}
