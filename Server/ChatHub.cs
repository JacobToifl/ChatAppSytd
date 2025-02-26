using Microsoft.AspNetCore.SignalR;

namespace Server;
public class ChatHub : Hub
{
    private static List<string> chatRooms = new() { "Generell" };
    private static readonly Dictionary<string, HashSet<string>> roomUsers = new();
    private static readonly Dictionary<string, string> userNames = new();
    
    public async Task SetName(string name)
    {
        if (!userNames.ContainsKey(Context.ConnectionId))
        {
            userNames[Context.ConnectionId] = name;
            await Clients.Caller.SendAsync("NameSet", name); 
        }
    }

    
    public async Task JoinRoom(string room)
    {
        if (!roomUsers.ContainsKey(room))
        {
            roomUsers[room] = new HashSet<string>();
        }
        roomUsers[room].Add(Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
        string userName = userNames.ContainsKey(Context.ConnectionId) ? userNames[Context.ConnectionId] : "Ein Benutzer";
        await Clients.Group(room).SendAsync("ReceiveMessage", "System", $"{userName} ist dem Raum beigetreten.");
    }


    public async Task LeaveRoom(string room)
    {
        string userName = userNames.ContainsKey(Context.ConnectionId) ? userNames[Context.ConnectionId] : "Ein Benutzer";

        if (roomUsers.ContainsKey(room))
        {
            roomUsers[room].Remove(Context.ConnectionId);
            if (roomUsers[room].Count == 0)
            {
                roomUsers.Remove(room);
            }
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
        await Clients.Group(room).SendAsync("ReceiveMessage", "System", $"{userName} hat den Raum verlassen.");
    }



    public Task<bool> IsRoomEmpty(string room)
    {
        return Task.FromResult(!roomUsers.ContainsKey(room) || roomUsers[room].Count == 0);
    }

    public async Task DeleteRoom(string room)
    {
        if (await IsRoomEmpty(room))
        {
            await Clients.All.SendAsync("RoomRemoved", room);
        }
    }
    
    public async Task SendMessage(string username, string message)
    {
        if(message == "")
            return;
        await Clients.All.SendAsync("ReceiveMessage",username,  message);
    }
        

    public async Task CreateRoom(string roomName)
    {
        if (!chatRooms.Contains(roomName))
        {
            chatRooms.Add(roomName);
            await Clients.All.SendAsync("RoomAdded", roomName);
        }
    }
}

