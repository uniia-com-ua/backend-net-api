using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using UniiaAdmin.Data.Constants;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.WebApi.Attributes;
using UniiaAdmin.WebApi.Resources;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("api/v1/roles")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AdminContext _adminContext;
        private readonly IPaginationService _paginationService;
        private readonly IStringLocalizer<ErrorMessages> _localizer;

        public RolesController
            (AdminContext adminContext,
            RoleManager<IdentityRole> signInManager,
            IPaginationService paginationService,
            IStringLocalizer<ErrorMessages> stringLocalizer)
        {
            _roleManager = signInManager;
			_adminContext = adminContext;
            _paginationService = paginationService;
            _localizer = stringLocalizer;
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
            var role = await _roleManager.FindByNameAsync(roleName);

            if (roleName == CustomRoles.AdminRole || roleName == CustomRoles.GuestRole)
            {
                return BadRequest(_localizer["RoleCannotBeDeleted"].Value);
            }

            if (role == null)
                return NotFound(_localizer["RoleNotFound", roleName].Value);

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpGet("page")]
        [Permission(PermissionResource.Role, CrudActions.View)]
        public async Task<IActionResult> GetAllRoles(int skip = 0, int take = 10)
        {
            var roles = await _paginationService.GetPagedListAsync(_roleManager.Roles, skip, take);

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
		public async Task<IActionResult> GetClaimsByRoleName(string roleName, int skip = 0, int take = 10)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
                return NotFound($"Role with name {roleName} not found");

            var roleClaims = await _roleManager.GetClaimsAsync(role);

			var claims = await _paginationService.GetPagedListAsync(_adminContext.RoleClaims
                .Where(r => r.RoleId == role.Id)
				.Select(rc => rc.ClaimValue!)
	            .Distinct()
	            .OrderBy(o => o), skip, take);

			return Ok(claims);
        }

        [HttpGet("claims/page")]
		[Permission(PermissionResource.Role, CrudActions.View)]
		public async Task<IActionResult> GetPaginatedClaims(int skip = 0, int take = 10)
        {
            var claims = await _paginationService.GetPagedListAsync(_adminContext.RoleClaims
                .Select(rc => rc.ClaimValue!)
				.Distinct()
                .OrderBy(o => o), skip, take);

            return Ok(claims);
        }
    }
}
