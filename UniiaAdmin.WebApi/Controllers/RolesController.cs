using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Interfaces;
using UniiaAdmin.WebApi.Resources;

namespace UniiaAdmin.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleManager;
        private readonly IStringLocalizer<ErrorMessages> _localizer;
		private readonly IRolePaginationService _rolePaginationService;

		public RolesController(
			IRoleRepository roleManager,
            IStringLocalizer<ErrorMessages> stringLocalizer,
            IRolePaginationService rolePaginationService)
        {
            _roleManager = roleManager;
            _localizer = stringLocalizer;
            _rolePaginationService = rolePaginationService;
		}

		[HttpPost("{roleName}/{claim}")]
		[Permission(PermissionResource.Role, CrudActions.Update)]
        public async Task<IActionResult> AddClaimToRole(string roleName, string claim)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                return NotFound(_localizer["RoleNotFound", roleName].Value);
            }

            var existingClaims = await _roleManager.GetClaimsAsync(role);

            if (existingClaims.Any(c => c.Type == CustomClaimTypes.Permission && c.Value == claim))
            {
                return Conflict(_localizer["ClaimExist", claim, roleName].Value);
            }

            var newClaim = new Claim(CustomClaimTypes.Permission, claim);

            var result = await _roleManager.AddClaimAsync(role, newClaim);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpDelete("{roleName}/{claim}")]
        [Permission(PermissionResource.Role, CrudActions.Update)]
        public async Task<IActionResult> RemoveClaimFromRole(string roleName, string claim)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                return NotFound(_localizer["RoleNotFound", roleName].Value);
            }

            var claimValue = claim.ToString();

            var existingClaims = await _roleManager.GetClaimsAsync(role);

            var roleClaim = existingClaims.FirstOrDefault(c => c.Value == claimValue);

            if (roleClaim == null)
                return NotFound(_localizer["ClaimNotExist", claim, roleName].Value);

            var result = await _roleManager.RemoveClaimAsync(role, roleClaim);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpPost]
        [Permission(PermissionResource.Role, CrudActions.Create)]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest(_localizer["RoleExist", roleName].Value);
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpDelete]
        [Permission(PermissionResource.Role, CrudActions.Delete)]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            if (roleName == CustomRoles.AdminRole || roleName == CustomRoles.GuestRole)
            {
                return BadRequest(_localizer["RoleCannotBeDeleted"].Value);
            }

			var role = await _roleManager.FindByNameAsync(roleName);

			if (role == null)
                return NotFound(_localizer["RoleNotFound", roleName].Value);

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpGet("page")]
        [Permission(PermissionResource.Role, CrudActions.View)]
        public async Task<IActionResult> GetAllRoles([FromQuery] string? sort = null, int skip = 0, int take = 10)
        {
            var roles = await _rolePaginationService.GetPagedRolesAsync(skip, take, sort);

            return Ok(roles);
        }

		[HttpGet("{roleName}")]
		[Permission(PermissionResource.Role, CrudActions.View)]
        public async Task<IActionResult> GetRoleByName(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
				return NotFound(_localizer["RoleNotFound", roleName].Value);

			return Ok(role);
        }

        [HttpGet("{roleName}/claims")]
		[Permission(PermissionResource.Role, CrudActions.View)]
		public async Task<IActionResult> GetClaimsByRoleName(string roleName, [FromQuery] int skip = 0, int take = 10, string? sort = null)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
                return NotFound($"Role with name {roleName} not found");

			var claims = await _rolePaginationService.GetPagedClaimsAsync(role.Id, skip, take, sort);

			return Ok(claims);
        }

        [HttpGet("claims/page")]
		[Permission(PermissionResource.Role, CrudActions.View)]
		public async Task<IActionResult> GetPaginatedClaims([FromQuery] int skip = 0, int take = 10, string? sort = null)
        {
            var claims = await _rolePaginationService.GetPagedClaimsAsync(skip, take, sort);

			return Ok(claims);
        }
    }
}
