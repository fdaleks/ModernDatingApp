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

    public async Task<bool> UserExists(string userName)
    {
        return await context.Users.AnyAsync(x => x.NormalizedUserName == userName.ToUpper());
    }

    // Users
    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        var user = await context.Users.FindAsync(id);
        return user;
    }

    public async Task<AppUser?> GetUserByNameAsync(string userName)
    {
        var user = await context.Users
            .Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.NormalizedUserName == userName.ToUpper());
        return user;
    }
    
    public async Task<AppUser?> GetUserByPhotoIdAsync(int photoId)
    {
        var user = await context.Users
            .IgnoreQueryFilters()
            .Include(x => x.Photos)
            .SingleOrDefaultAsync(x => x.Photos.Any(p => p.Id == photoId));
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

    public async Task<MemberDto?> GetMemberByNameAsync(string userName, bool isCurrentUser)
    {
        var memberQuery = isCurrentUser 
            ? context.Users.IgnoreQueryFilters()
            : context.Users.AsQueryable();

        var member = await memberQuery
            .Where(x => x.NormalizedUserName == userName.ToUpper())
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

        return member;
    }
}
