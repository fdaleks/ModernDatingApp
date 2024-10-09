using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers.Params;
using API.Interfaces;
using AutoMapper.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class LikesController(ILikesRepository likesRepository) : BaseApiController
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        var sourceUserId = User.GetUserId();

        if (sourceUserId == targetUserId) return BadRequest("You can't lick your own balls");

        var existingLike = await likesRepository.GetUserLikeAsync(sourceUserId, targetUserId);
        if (existingLike == null)
        {
            var newLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            };

            likesRepository.AddLike(newLike);
        }
        else
        {
            likesRepository.DeleteLike(existingLike);
        }

        if (await likesRepository.SaveAllAsync()) return Ok();
        return BadRequest("Failed to update like");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery] LikesParams userParams)
    {
        userParams.UserId = User.GetUserId();

        var users = await likesRepository.GetUserLikesAsync(userParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
    {
        var sourceUserId = User.GetUserId();

        var result = await likesRepository.GetCurrentUserLikeIdsAsync(sourceUserId);

        return Ok(result);
    }
}
