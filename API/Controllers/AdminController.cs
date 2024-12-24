using API.Entities;
using API.Interfaces;
using API.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, 
    IHubContext<PresenceHub> presenceHub) : BaseApiController
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
        var photos = await unitOfWork.PhotoRepository.GetPhotosForModerationAsync();
        return Ok(photos);
    }
    
    [Authorize(Policy = "ModeratePhotosRole")]
    [HttpPut("approve-photo/{photoId:int}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoByIdAsync(photoId);
        if (photo == null) return BadRequest("Can't find photo for approval");

        photo.IsModerated = true;
        photo.IsApproved = true;

        var user = await unitOfWork.UserRepository.GetUserByPhotoIdAsync(photoId);
        if (user == null) return BadRequest("Can't find user related to this photo");

        var hasMainPhoto = user.Photos.Any(x => x.IsMain);
        if (!hasMainPhoto)
        {
            photo.IsMain = true;
            var connections = await PresenceTracker.GetConnectionsForUserAsync(user.UserName!);
            if (connections != null && connections?.Count != null)
            {
                await presenceHub.Clients.Clients(connections).SendAsync("UpdateMainPhoto", photo.Url);
            }
        }

        if (await unitOfWork.CompleteAsync()) return NoContent();
        return BadRequest("Failed to approve photo");
    }

    [Authorize(Policy = "ModeratePhotosRole")]
    [HttpPut("reject-photo/{photoId:int}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoByIdAsync(photoId);
        if (photo == null) return BadRequest("Can't find photo for approval");

        photo.IsModerated = true;
        photo.IsApproved = false;

        if (await unitOfWork.CompleteAsync()) return NoContent();
        return BadRequest("Failed to reject photo approval");
    }
    
}
