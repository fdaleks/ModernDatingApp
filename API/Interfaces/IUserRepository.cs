using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Helpers.Params;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<bool> SaveAllAsync();

    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByIdAsync(int id);
    Task<AppUser?> GetUserByNameAsync(string name);

    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto?> GetMemberByIdAsync(int id);
    Task<MemberDto?> GetMemberByNameAsync(string name);
}
