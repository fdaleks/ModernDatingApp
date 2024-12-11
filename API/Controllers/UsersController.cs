using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers.Params;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IPhotoService photoService, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers([FromQuery] UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUserName();

        var members = await unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(members);

        return Ok(members);
    }

    [HttpGet("{userName}")]
    public async Task<ActionResult<MemberDto>> GetMember(string userName)
    {
        bool isCurrentUser = userName == User.GetUserName();
        var member = await unitOfWork.UserRepository.GetMemberByNameAsync(userName, isCurrentUser);

        if (member == null) return NotFound();
        return Ok(member);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var currentUser = await unitOfWork.UserRepository.GetUserByIdAsync(User.GetUserId());
        if (currentUser == null) return BadRequest("Could not find user");

        mapper.Map(memberUpdateDto, currentUser);

        if (await unitOfWork.CompleteAsync()) return NoContent();
        return BadRequest("Failed to update the user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var currentUser = await unitOfWork.UserRepository.GetUserByNameAsync(User.GetUserName());
        if (currentUser == null) return BadRequest("User not found");

        var result = await photoService.AddPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
        };

        currentUser.Photos.Add(photo);

        if (await unitOfWork.CompleteAsync()) 
            return CreatedAtAction(nameof(GetMember), new { userName = currentUser.UserName }, mapper.Map<PhotoDto>(photo));

        return BadRequest("Problems with adding photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var currentUser = await unitOfWork.UserRepository.GetUserByNameAsync(User.GetUserName());
        if (currentUser == null) return BadRequest("User not found");

        var photo = currentUser.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null || photo.IsMain) return BadRequest("Can't set this photo as main");

        var currentmainPhoto = currentUser.Photos.FirstOrDefault(x => x.IsMain);
        if (currentmainPhoto != null) currentmainPhoto.IsMain = false;

        photo.IsMain = true;

        if (await unitOfWork.CompleteAsync()) return NoContent();
        return BadRequest("Failed to set new main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> RemovePhoto(int photoId)
    {
        var photo = await unitOfWork.PhotoRepository.GetPhotoByIdAsync(photoId);
        if (photo == null || photo.IsMain) return BadRequest("Can't remove this photo");

        if (photo.AppUserId != User.GetUserId()) return BadRequest("You can't delete another user's photo");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photoId.ToString());
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        unitOfWork.PhotoRepository.RemovePhoto(photo);

        if (await unitOfWork.CompleteAsync()) return Ok();
        return BadRequest("Failed to delete photo");
    }
}