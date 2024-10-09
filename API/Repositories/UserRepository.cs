using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Helpers.Params;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

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

    // Users
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

    // Members
    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

        var usersQuery = context.Users.AsQueryable();

        usersQuery = usersQuery.Where(x => x.UserName != userParams.CurrentUsername);
        usersQuery = usersQuery.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

        if (userParams.Gender != null)
        {
            usersQuery = usersQuery.Where(x => x.Gender == userParams.Gender);
        }

        usersQuery = userParams.OrderBy switch
        {
            "createdOn" => usersQuery.OrderByDescending(x => x.CreatedOn),
            "lastActive" or _ => usersQuery.OrderByDescending(x => x.LastActive)
        };

        var membersQuery = usersQuery.ProjectTo<MemberDto>(mapper.ConfigurationProvider);
        var result = await PagedList<MemberDto>.CreateAsync(membersQuery, userParams.PageNumber, userParams.PageSize);

        return result;
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
