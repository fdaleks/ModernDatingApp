namespace API.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> OnlineUsers = [];

    public Task<bool> UserConnectedAsync(string userName, string connectionId)
    {
        var isOnline = false;
        lock (OnlineUsers) 
        {
            if (OnlineUsers.ContainsKey(userName))
            {
                OnlineUsers[userName].Add(connectionId);
            } 
            else
            {
                OnlineUsers.Add(userName, [connectionId]);
                isOnline = true;
            }
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnectedAsync(string userName, string connectionId)
    {
        var isOffline = false;
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(userName)) return Task.FromResult(isOffline);

            OnlineUsers[userName].Remove(connectionId);
            if (OnlineUsers[userName].Count == 0)
            {
                OnlineUsers.Remove(userName);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
    }

    public Task<string[]> GetOnlineUsersAsync()
    {
        string[] onlineUsers;
        lock (OnlineUsers) 
        {
            onlineUsers = OnlineUsers.OrderBy(x => x.Key).Select(x => x.Key).ToArray();
        }

        return Task.FromResult(onlineUsers);
    }

    public static Task<List<string>> GetConnectionsForUserAsync(string userName)
    {
        List<string> connectionIds;

        if (OnlineUsers.TryGetValue(userName, out var connections))
        {
            lock (connections)
            {
                connectionIds = [.. connections];
            }
        }
        else
        {
            connectionIds = [];
        }

        return Task.FromResult(connectionIds);
    }
}
