using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(IUserRepository userRepository, IPhotoService photoService, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await userRepository.GetMembersAsync();

        return Ok(users);
    }

    [HttpGet("{userName}")]
    public async Task<ActionResult<MemberDto>> GetUser(string userName)
    {
        var user = await userRepository.GetMemberByNameAsync(userName);

        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await userRepository.GetUserByNameAsync(User.GetUserName());
        if (user == null) return BadRequest("Could not find user");

        mapper.Map(memberUpdateDto, user);

        if (await userRepository.SaveAllAsync()) return NoContent();
        return BadRequest("Failed to update the user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await userRepository.GetUserByNameAsync(User.GetUserName());
        if (user == null) return BadRequest("User not found");

        var result = await photoService.AddPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        if (user.Photos.Count == 0)
        {
            photo.IsMain = true;
        }

        user.Photos.Add(photo);

        if (await userRepository.SaveAllAsync()) 
            return CreatedAtAction(nameof(GetUser), new { userName = user.UserName }, mapper.Map<PhotoDto>(photo));

        return BadRequest("Problems with adding photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await userRepository.GetUserByNameAsync(User.GetUserName());
        if (user == null) return BadRequest("User not found");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null || photo.IsMain) return BadRequest("Can't set this photo as main");

        var currentmainPhoto = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentmainPhoto != null) currentmainPhoto.IsMain = false;

        photo.IsMain = true;

        if (await userRepository.SaveAllAsync()) return NoContent();
        return BadRequest("Failed to set new main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> RemovePhoto(int photoId)
    {
        var user = await userRepository.GetUserByNameAsync(User.GetUserName());
        if (user == null) return BadRequest("User not found");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null || photo.IsMain) return BadRequest("Can't remove this photo");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photoId.ToString());
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await userRepository.SaveAllAsync()) return Ok();
        return BadRequest("Failed to delete photo");
    }
}