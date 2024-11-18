using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class MessagesHub(IMessagesRepository messagesRepository, IUserRepository userRepository, 
    IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
{
    public async override Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();

        var recipientUserName = httpContext?.Request.Query["user"];

        if (Context.User == null || string.IsNullOrEmpty(recipientUserName)) throw new HubException("Can't join a group chat");

        var currentUserName = Context.User.GetUserName();
        var groupName = GetGroupName(currentUserName, recipientUserName!);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToMessageGroupAsync(groupName);

        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await messagesRepository.GetMessageThreadAsync(currentUserName, recipientUserName!);
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromMessageGroupAsync();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(MessageCreateDto dto)
    {
        var currentUserName = Context.User?.GetUserName() ?? throw new HubException("Can't get userName");
        if (currentUserName == dto.RecipientUserName.ToLower()) throw new HubException("You can't message yourself");

        var sender = await userRepository.GetUserByNameAsync(currentUserName);
        var recipient = await userRepository.GetUserByNameAsync(dto.RecipientUserName);

        if (sender == null || sender.UserName == null || recipient == null || recipient.UserName == null)
            throw new HubException("Can't send a message");

        var message = new Message
        {
            Sender = sender,
            SenderUserName = sender.UserName,
            Recipient = recipient,
            RecipientUserName = recipient.UserName,
            Content = dto.Content,
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await messagesRepository.GetMessageGroupAsync(groupName);

        if (group != null && group.Connections.Any(x => x.UserName == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUserAsync(recipient.UserName);
            if (connections != null && connections?.Count != null)
            {
                await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", 
                    new { userName = sender.UserName, knownAs = sender.KnownAs });
            }
        }

        messagesRepository.AddMessage(message);

        if (await messagesRepository.SaveAllAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
    }

    private async Task<Group> AddToMessageGroupAsync(string groupName)
    {
        var currentUserName = Context.User?.GetUserName() ?? throw new HubException("Can't get userName");
        var group = await messagesRepository.GetMessageGroupAsync(groupName);

        var connection = new Connection 
        { 
            ConnectionId = Context.ConnectionId,
            UserName = currentUserName
        };
        
        if (group == null)
        {
            group = new Group { Name = groupName };
            messagesRepository.AddGroup(group);
        }

        group.Connections.Add(connection);

        if (await messagesRepository.SaveAllAsync()) return group;
        throw new HubException("Failed to join message group");
    }

    private async Task<Group> RemoveFromMessageGroupAsync()
    {
        var group = await messagesRepository.GetGroupForConnectionAsync(Context.ConnectionId);
        var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

        if (connection != null && group != null)
        {
            messagesRepository.RemoveConnection(connection);
            if (await messagesRepository.SaveAllAsync()) return group;
        }

        throw new HubException("Failed to remove from message group");
    }

    private static string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        string result = stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        return result;
    }
}
