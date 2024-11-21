using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Helpers.Params;

namespace API.Interfaces;

public interface ILikesRepository
{
    void AddLike(UserLike like);
    void DeleteLike(UserLike like);

    Task<UserLike?> GetUserLikeAsync(int sourceUserId, int targetUserId);
    Task<PagedList<MemberDto>> GetUserLikesAsync(LikesParams likesParams);
    Task<IEnumerable<int>> GetCurrentUserLikeIdsAsync(int currentUserId);
}
