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

public class MessagesRepository(DataContext context, IMapper mapper) : IMessagesRepository
{
    public void AddMessage(Message message)
    {
        context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
        context.Messages.Remove(message);
    }

    public async Task<Message?> GetMessageByIsAsync(int messageId)
    {
        var result = await context.Messages.FindAsync(messageId);
        return result;
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUserAsync(MessageParams messageParams)
    {
        var messagesQuery = context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();

        messagesQuery = messageParams.Container switch 
        {
            "inbox" => messagesQuery.Where(x => x.RecipientUserName == messageParams.UserName 
                && x.RecipientDeleted == false),

            "outbox" => messagesQuery.Where(x => x.SenderUserName == messageParams.UserName 
                && x.SenderDeleted == false),

            "unread" or _ => messagesQuery.Where(x => x.RecipientUserName == messageParams.UserName && x.DateRead == null
                && x.RecipientDeleted == false)
        };

        var messages = await PagedList<MessageDto>.CreateAsync(messagesQuery.ProjectTo<MessageDto>(mapper.ConfigurationProvider), 
            messageParams.PageNumber, messageParams.PageSize);
        return messages;
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUserName, string recipientUserName)
    {
        var messagesQuery = context.Messages
            .Where(x =>
                (x.RecipientUserName == currentUserName && x.RecipientDeleted == false && x.SenderUserName == recipientUserName) ||
                (x.SenderUserName == currentUserName && x.SenderDeleted == false && x.RecipientUserName == recipientUserName)
            )
            .OrderBy(x => x.MessageSent)
            .AsQueryable();

        var unredMessages = messagesQuery.Where(x => x.DateRead == null && x.RecipientUserName == currentUserName).ToList();
        if (unredMessages.Count != 0) unredMessages.ForEach(x => x.DateRead = DateTime.UtcNow);

        var messages = await messagesQuery.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
        return messages;
    }

    public void AddGroup(Group group)
    {
        context.Groups.Add(group);
    }

    public async Task<Group?> GetMessageGroupAsync(string groupName)
    {
        var result = await context.Groups
            .Include(x => x.Connections)
            .FirstOrDefaultAsync(x => x.Name == groupName);
        return result;
    }

    public async Task<Group?> GetGroupForConnectionAsync(string connectionId)
    {
        var result = await context.Groups
            .Include(x => x.Connections)
            .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
            .FirstOrDefaultAsync();
        return result;
    }

    public void RemoveConnection(Connection connection)
    {
        context.Connections.Remove(connection);
    }
}
