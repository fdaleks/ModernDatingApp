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

    public async Task<bool> SaveAllAsync()
    {
        var result = await context.SaveChangesAsync();
        return result > 0;
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

        var messages = messagesQuery.ProjectTo<MessageDto>(mapper.ConfigurationProvider);

        var result = await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        return result;
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThreadAsync(string currentUserName, string recipientUserName)
    {
        // rewrite later
        var messages = await context.Messages
            .Include(x => x.Sender).ThenInclude(x => x.Photos)
            .Include(x => x.Recipient).ThenInclude(x => x.Photos)
            .Where(x => 
                (x.RecipientUserName == currentUserName && x.RecipientDeleted == false && x.SenderUserName == recipientUserName) ||
                (x.SenderUserName == currentUserName && x.SenderDeleted == false && x.RecipientUserName == recipientUserName)
            ).OrderBy(x => x.MessageSent).ToListAsync();

        var unredMessages = messages.Where(x => x.DateRead == null && x.RecipientUserName == currentUserName).ToList();
        if (unredMessages.Count != 0)
        {
            unredMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
            await context.SaveChangesAsync();
        }

        var result = mapper.Map<IEnumerable<MessageDto>>(messages);
        return result;
    }
}
