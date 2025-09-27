using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Dtos.UserDtos;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Interfaces.IUnitOfWork;
using UniiaAdmin.WebApi.Repository;
using UniiaAdmin.WebApi.Resources;

namespace UniiaAdmin.WebApi.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
		private readonly IApplicationUnitOfWork _applicationUnitOfWork;
		private readonly IStringLocalizer<ErrorMessages> _localizer;
		private readonly IMapper _mapper;
		public UserController(
			IUserRepository userRepository,
			IStringLocalizer<ErrorMessages> localizer,
			IApplicationUnitOfWork applicationUnitOfWork,
			IMapper mapper) 
        {
			_userRepository = userRepository;
			_applicationUnitOfWork = applicationUnitOfWork;
			_localizer = localizer;
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

		[HttpPost]
		[Permission(PermissionResource.User, CrudActions.Create)]
		public async Task<IActionResult> Create([FromBody] UserDto userDto)
		{
			if (string.IsNullOrEmpty(userDto.Email))
			{
				return NotFound(_localizer["EmailRequired"].Value);
			}

			if (await _userRepository.IsEmailExistAsync(userDto.Email))
			{
				return NotFound(_localizer["EmailExist", userDto.Email].Value);
			}

			var id = await _userRepository.CreateAsync(userDto);

			HttpContext.Items.Add("id", id);

			return Ok();
		}

		[HttpPost("{id}")]
		[Permission(PermissionResource.User, CrudActions.Update)]
		public async Task<IActionResult> Update([FromBody] UserDto userDto, string id)
		{
			await _userRepository.UpdateAsync(id, userDto);

			HttpContext.Items.Add("id", id);

			return Ok();
		}

		[HttpGet("page")]
		[Permission(PermissionResource.User, CrudActions.View)]
		public async Task<IActionResult> GetAllUsers([FromQuery] int skip = 0, int take = 10)
        {
            var users = await _applicationUnitOfWork.GetPagedAsync<User>(skip, take);

			var result = users.Select(u => _mapper.Map<UserDto>(u));

            return Ok(result);
        }

        [HttpGet("{id}")]
		[Permission(PermissionResource.User, CrudActions.View)]
		public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _applicationUnitOfWork.FindAsync<User>(id);

            if (user == null)
                return NotFound(_localizer["ModelNotFound", nameof(Data.Models.User), id.ToString()].Value);

			var result = _mapper.Map<UserDto>(user);

            return Ok(result);
        }
    }
}
