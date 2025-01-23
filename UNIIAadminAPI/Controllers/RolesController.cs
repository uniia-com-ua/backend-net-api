using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UNIIAadminAPI.Enums;
using UNIIAadminAPI.Services;

namespace UNIIAadminAPI.Controllers
{
    [ApiController]
    [Route("admin/api/roles")]
    public class RolesController(RoleManager<MongoIdentityRole> roleManager) : ControllerBase
    {
        [HttpPatch]
        [Route("add-to-role")]
        [ValidateToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddClaimToRole(string roleName, ClaimsEnum claim)
        {
            var role = await roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                return NotFound();
            }

            var claimValue = claim.ToString();

            var existingClaims = await roleManager.GetClaimsAsync(role);

            if (existingClaims.Any(c => c.Type == ClaimTypes.Role && c.Value == claimValue))
            {
                return Conflict();
            }

            var newClaim = new Claim(ClaimTypes.Role, claimValue);

            var result = await roleManager.AddClaimAsync(role, newClaim);

            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPatch]
        [Route("remove-from-role")]
        [ValidateToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveClaimFromRole(string roleName, ClaimsEnum claim)
        {
            var role = await roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                return NotFound();
            }

            var claimValue = claim.ToString();

            var existingClaims = await roleManager.GetClaimsAsync(role);

            var roleClaim = existingClaims.FirstOrDefault(c => c.Type == ClaimTypes.Role && c.Value == claimValue);

            if (roleClaim == null)
                return NotFound();

            role.RemoveClaim(roleClaim);

            await roleManager.UpdateAsync(role);

            return Ok();
        }

        [HttpPost]
        [Route("create")]
        [ValidateToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var result = await roleManager.CreateAsync(new MongoIdentityRole(roleName));

            if (!result.Succeeded)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpDelete]
        [Route("delete")]
        [ValidateToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await roleManager.FindByNameAsync(roleName);

            if (role == null)
                return NotFound();

            var result = await roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return BadRequest();

            return Ok();
        }

        [HttpGet]
        [Route("get-all")]
        [ValidateToken]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllRoles()
        {
            var roles = roleManager.Roles;

            if (roles == null)
                BadRequest();

            return Ok(roles);
        }

        [HttpGet]
        [Route("get-role-by-name")]
        [ValidateToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoleByName(string roleName)
        {
            var role = await roleManager.FindByNameAsync(roleName);

            if (role == null)
                BadRequest();

            return Ok(role);
        }
    }
}
