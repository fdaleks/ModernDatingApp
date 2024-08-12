using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository(DataContext context, IMapper mapper) : IUserRepository
{
    public void Update(AppUser user)
    {
        context.Entry(user).State = EntityState.Modified;
    }
    public async Task<bool> SaveAllAsync()
    {
        var result = await context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        var users = await context.Users
            .Include(x => x.Photos)
            .ToListAsync();
        return users;
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        var user = await context.Users.FindAsync(id);
        return user;
    }

    public async Task<AppUser?> GetUserByNameAsync(string userName)
    {
        var user = await context.Users
            .Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.UserName.ToLower() == userName.ToLower());
        return user;
    }


    public async Task<IEnumerable<MemberDto>> GetMembersAsync()
    {
        var members = await context.Users
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .ToListAsync();

        return members;
    }

    public async Task<MemberDto?> GetMemberByIdAsync(int id)
    {
        var member = await context.Users
            .Where(x => x.Id == id)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

        return member;
    }

    public async Task<MemberDto?> GetMemberByNameAsync(string userName)
    {
        var member = await context.Users
            .Where(x => x.UserName.ToLower() == userName.ToLower())
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

        return member;
    }
}
