using Microsoft.AspNetCore.SignalR;
using Silmoon.AspNetCore.Test.Services;

namespace Silmoon.AspNetCore.Test.Hubs
{
    public class ChatServiceHub : Hub
    {
        private readonly ChatService _chatService;

        public ChatServiceHub(ChatService chatService)
        {
            _chatService = chatService;
        }

        public Task SendToMe(string message) => _chatService.SendToMe(Context.ConnectionId, message);

        public Task SendToAll(string message) => _chatService.SendToAll(Context.ConnectionId, message);

        public Task SendToUser(string username, string message) => _chatService.SendToUser(Context.ConnectionId, username, message);

        public Task UserSignin(string username) => _chatService.UserSignin(Context.ConnectionId, username);

        public Task UserSignout(string username) => _chatService.UserSignout(username);

        public override Task OnDisconnectedAsync(Exception? exception) => _chatService.HandleDisconnect(Context.ConnectionId);
    }
}
