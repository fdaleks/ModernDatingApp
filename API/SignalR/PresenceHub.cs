using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker presenceTracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User == null) throw new HubException("Can't get current user's claims");

        var isOnline = await presenceTracker.UserConnectedAsync(Context.User.GetUserName(), Context.ConnectionId);
        if (isOnline) await Clients.Others.SendAsync("UserIsOnline", Context.User?.GetUserName());

        var currentUsers = await presenceTracker.GetOnlineUsersAsync();
        await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User == null) throw new HubException("Can't get current user's claims");

        var isOffline = await presenceTracker.UserDisconnectedAsync(Context.User.GetUserName(), Context.ConnectionId);
        if (isOffline) await Clients.Others.SendAsync("UserIsOffline", Context.User?.GetUserName());

        await base.OnDisconnectedAsync(exception);
    }
}
