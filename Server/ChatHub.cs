using Microsoft.AspNetCore.SignalR;

namespace Server;
public class ChatHub : Hub
{
    public async Task SetName(string username)
    {
        if (UsernameStorage.Storage.ContainsValue(username))
        {
            await Clients.Caller.SendAsync("NameExistsError");
            return;
        }
        
        UsernameStorage.Storage[Context.ConnectionId] = username;
        await Clients.Caller.SendAsync("NameSet", username);
    }

    public async Task JoinRoom(string roomName)
    {
        if (!UsernameStorage.Storage.TryGetValue(Context.ConnectionId, out var username))
        {
            await Clients.Caller.SendAsync("Error", "Name not set");
            return;
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("ReceiveMessage", "System", $"{username} ist dem Raum '{roomName}' beigetreten.");
    }

    public async Task LeaveRoom(string roomName)
    {
        if (UsernameStorage.Storage.TryGetValue(Context.ConnectionId, out var username))
        {
            await Clients.Group(roomName).SendAsync("ReceiveMessage", "System", $"{username} hat den Raum '{roomName}' verlassen.");
        }
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
    }
    
    public async Task SendMessage(string username, string message)
    {
        if(message == "")
            return;
        await Clients.All.SendAsync("ReceiveMessage",username,  message);
    }
        
    private static List<string> chatRooms = new() { "Generell" };

    public async Task CreateRoom(string roomName)
    {
        if (!chatRooms.Contains(roomName))
        {
            chatRooms.Add(roomName);
            await Clients.All.SendAsync("RoomAdded", roomName);
        }
    }

    public async Task DeleteRoom(string roomName)
    {
        if (chatRooms.Contains(roomName))
        {
            chatRooms.Remove(roomName);
            await Clients.All.SendAsync("RoomRemoved", roomName);
        }
    }
}

