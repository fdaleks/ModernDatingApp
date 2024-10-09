using API.Data;
using API.DTOs;
using API.Entities;
using API.Helpers.Params;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories;

public class LikesRepository(DataContext context, IMapper mapper) : ILikesRepository
{
    public void AddLike(UserLike like)
    {
        context.Likes.Add(like);
    }

    public void DeleteLike(UserLike like)
    {
        context.Likes.Remove(like);
    }

    public async Task<bool> SaveAllAsync()
    {
        var result = await context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<IEnumerable<int>> GetCurrentUserLikeIdsAsync(int currentUserId)
    {
        var result  = await context.Likes
            .Where(x => x.SourceUserId == currentUserId)
            .Select(x => x.TargetUserId)
            .ToListAsync();

        return result;
    }

    public async Task<UserLike?> GetUserLikeAsync(int sourceUserId, int targetUserId)
    {
        var result = await context.Likes.FindAsync(sourceUserId, targetUserId);
        return result;
    }

    public async Task<PagedList<MemberDto>> GetUserLikesAsync(LikesParams likesParams)
    {
        var likesQuery = context.Likes.AsQueryable();
        IQueryable<MemberDto> membersQuery;

        switch (likesParams.Predicate)
        {
            case "liked":
                membersQuery = likesQuery
                    .Where(x => x.SourceUserId == likesParams.UserId)
                    .Select(x => x.TargetUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
            case "likedBy":
                membersQuery = likesQuery
                    .Where(x => x.TargetUserId == likesParams.UserId)
                    .Select(x => x.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
            case "mutual":
            default:
                var likeIds = await GetCurrentUserLikeIdsAsync(likesParams.UserId);
                membersQuery = likesQuery
                    .Where(x => x.TargetUserId == likesParams.UserId && likeIds.Contains(x.SourceUserId))
                    .Select(x => x.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                break;
        }

        var result = await PagedList<MemberDto>.CreateAsync(membersQuery, likesParams.PageNumber, likesParams.PageSize);
        return result;
    }

    /*
    public async Task<IEnumerable<MemberDto>> GetUserLikesAsync(string predicate, int userId)
    {
        var likes = context.Likes.AsQueryable();

        switch (predicate) 
        {
            case "liked":
                return await likes
                    .Where(x => x.SourceUserId == userId)
                    .Select(x => x.TargetUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
            case "likedBy":
                return await likes
                    .Where(x => x.TargetUserId == userId)
                    .Select(x => x.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
            case "mutual":
            default:
                var likeIds = await GetCurrentUserLikeIdsAsync(userId);
                return await likes
                    .Where(x => x.TargetUserId == userId && likeIds.Contains(x.SourceUserId))
                    .Select(x => x.SourceUser)
                    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
        }
    }
    */
}
