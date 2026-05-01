using Microsoft.AspNetCore.SignalR;

namespace AiBoard.Api.Hubs;

public sealed class BoardHub : Hub
{
    public Task JoinBoard(string boardId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, boardId);
    }

    // Allow trusted internal clients (worker) to notify the API which will broadcast to connected clients
    public Task NotifyNodeUpdated(string boardId, object payload)
    {
        return Clients.Group(boardId).SendAsync("NodeUpdated", payload);
    }
}
