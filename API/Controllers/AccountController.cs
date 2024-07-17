using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        if (await UserExists(dto.UserName)) return BadRequest("This username is already taken");

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = dto.UserName.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = new UserDto()
        {
            UserName = user.UserName,
            Token = tokenService.CreateToken(user)
        };

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.UserName.ToLower() == dto.UserName.ToLower());

        if (user == null) return Unauthorized("Invalid username");

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));

        for (int i = 0; i < computedHash.Length; i++) 
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return Unauthorized("Invalid password");
            }
        }

        var result = new UserDto()
        {
            UserName = user.UserName,
            Token = tokenService.CreateToken(user)
        };

        return Ok(result);
    }

    private async Task<bool> UserExists(string userName)
    {
        return await context.Users.AnyAsync(x => x.UserName.ToLower() == userName.ToLower());
    }
}
