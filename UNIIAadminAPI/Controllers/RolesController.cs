using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniiaAdmin.Data.Data;
using UniiaAdmin.Data.Interfaces;
using UniiaAdmin.Data.Models;
using UniiaAdmin.WebApi.Services;

namespace UNIIAadminAPI.Controllers
{
	[Authorize]
	[ApiController]
    [Route("api/v1/roles")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationContext _applicationContext;
		private readonly IPaginationService _paginationService;

		public RolesController
            (ApplicationContext applicationContext,
            RoleManager<IdentityRole> signInManager,
			IPaginationService paginationService)
        {
            _roleManager = signInManager;
            _applicationContext = applicationContext;
            _paginationService = paginationService;
        }

        [HttpPatch]
        [Route("add-claim-to-role")]
		public async Task<IActionResult> AddClaimToRole(string roleName, string claim)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                return NotFound($"Role with name {roleName} not found");
            }

            var existingClaims = await _roleManager.GetClaimsAsync(role);

            if (existingClaims.Any(c => c.Type == ClaimTypes.Role && c.Value == claim))
            {
                return Conflict();
            }

            var newClaim = new Claim(ClaimTypes.Role, claim);

            var result = await _roleManager.AddClaimAsync(role, newClaim);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpPatch]
        [Route("remove-from-role")]
        public async Task<IActionResult> RemoveClaimFromRole(string roleName, string claim)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                return NotFound($"Role with name {roleName} not found");
            }

            var claimValue = claim.ToString();

            var existingClaims = await _roleManager.GetClaimsAsync(role);

            var roleClaim = existingClaims.FirstOrDefault(c => c.Value == claimValue);

            if (roleClaim == null)
                return NotFound($"Role with name {roleName} and claim {claim} not found");

            await _roleManager.RemoveClaimAsync(role, roleClaim);

            return Ok();
        }

        [HttpPost]
        [Route("create-role")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpDelete]
        [Route("delete-role")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
                return NotFound($"Role with name {roleName} not found");

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return BadRequest();

            return Ok();
        }

        [HttpGet]
        [Route("get-paginated-roles")]
        public async Task<IActionResult> GetAllRoles(int skip = 0, int take = 10)
        {
            var roles = await _paginationService.GetPagedListAsync(_roleManager.Roles, skip, take);

            return Ok(roles);
        }

        [HttpGet]
        [Route("get-role-by-name")]
        public async Task<IActionResult> GetRoleByName(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
                return NotFound($"Role with name {roleName} not found");

            return Ok(role);
        }

        [HttpGet]
        [Route("get-claims-by-role")]
        public async Task<IActionResult> GetClaimsByRoleName(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
                return NotFound($"Role with name {roleName} not found");

            var claims = await _roleManager.GetClaimsAsync(role);

            return Ok(claims);
        }

        [HttpGet]
        [Route("get-paginated-claims")]
        public async Task<IActionResult> GetPaginatedClaims(int skip, int take)
        {
			var claims = await _paginationService.GetPagedListAsync(_applicationContext.RoleClaims, skip, take);

            return Ok(claims);
        }
    }
}
