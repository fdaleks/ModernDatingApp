using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Helpers.Params;

namespace API.Interfaces;

public interface IMessagesRepository
{
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<bool> SaveAllAsync();

    Task<Message?> GetMessageByIsAsync(int messageId);
    Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams);
    Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUserName, string recipientUserName);

    void AddGroup(Group group);
    Task<Group?> GetMessageGroupAsync(string groupName);
    Task<Group?> GetGroupForConnectionAsync(string connectionId);
    //Task<Connection?> GetConnectionAsync(string connectionId);
    void RemoveConnection(Connection connection);
}
