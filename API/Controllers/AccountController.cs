using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountController(UserManager<AppUser> userManager, 
    ITokenService tokenService, IUserRepository userRepository, IMapper mapper) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        if (await userRepository.UserExists(dto.UserName)) return BadRequest("This username is already taken");

        var user = mapper.Map<AppUser>(dto);
        user.UserName = dto.UserName.ToLower();

        var response = await userManager.CreateAsync(user, dto.Password);
        if (!response.Succeeded) return BadRequest(response.Errors);

        var result = new UserDto()
        {
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            Token = await tokenService.CreateToken(user)
        };

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto)
    {
        var user = await userRepository.GetUserByNameAsync(dto.UserName);
        if (user == null || user.UserName == null) return Unauthorized("Invalid username");

        var response = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!response) return Unauthorized();

        var result = new UserDto()
        {
            UserName = user.UserName,
            KnownAs = user.KnownAs,
            Gender = user.Gender,
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            Token = await tokenService.CreateToken(user)
        };

        return Ok(result);
    }
}
