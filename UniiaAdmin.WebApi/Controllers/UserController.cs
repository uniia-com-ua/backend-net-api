using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces;

namespace UniiaAdmin.WebApi.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IPaginationService _paginationService;
		private readonly IMapper _mapper;
		public UserController(
            ApplicationContext applicationContext,
            IPaginationService paginationService,
            IMapper mapper) 
        {
            _applicationContext = applicationContext;
            _paginationService = paginationService;
            _mapper = mapper;
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

		[HttpGet]
		[Permission(PermissionResource.User, CrudActions.View)]
		public async Task<IActionResult> GetAllUsers([FromQuery] int skip = 0, int take = 10)
        {
            var users = await _paginationService.GetPagedListAsync(_applicationContext.Users, skip, take);

			var result = users.Select(u => _mapper.Map<UserDto>(u));

            return Ok(result);
        }

        [HttpGet("{id}")]
		[Permission(PermissionResource.User, CrudActions.View)]
		public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _applicationContext.Users.FindAsync(id);

            if (user == null)
                return NotFound();

			var result = _mapper.Map<UserDto>(user);

            return Ok(result);
        }
    }
}
