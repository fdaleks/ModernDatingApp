using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUserWithRoles()
    {
        // should it be moved to the repository?
        var users = await userManager.Users
            .OrderBy(x => x.UserName)
            .Select(x => new 
            { 
                x.Id,
                x.UserName, 
                Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
            }).ToListAsync();

        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{userName}")]
    public async Task<ActionResult> EditRoles(string userName, string roles)
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("Role is empty");

        var selectedRoles = roles.Split(',').ToArray();

        var user = await userManager.FindByNameAsync(userName);
        if (user == null) return BadRequest("Can't find user");

        var userRoles = await userManager.GetRolesAsync(user);

        var response = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!response.Succeeded) return BadRequest("Failed to add to roles");

        response = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!response.Succeeded) return BadRequest("Failed to remove from roles");

        var result = await userManager.GetRolesAsync(user);
        return Ok(result);
    }

    [Authorize(Policy = "ModeratePhotosRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult> GetPhotosForModeration()
    {
        return Ok("Admins and moderators can see this");
    }
}
