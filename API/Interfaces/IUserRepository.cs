using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<bool> SaveAllAsync();

    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser?> GetUserByIdAsync(int id);
    Task<AppUser?> GetUserByNameAsync(string name);

    Task<IEnumerable<MemberDto>> GetMembersAsync();
    Task<MemberDto?> GetMemberByIdAsync(int id);
    Task<MemberDto?> GetMemberByNameAsync(string name);
}
