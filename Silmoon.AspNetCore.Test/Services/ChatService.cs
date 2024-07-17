using Microsoft.AspNetCore.SignalR;
using Silmoon.AspNetCore.Test.Hubs;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Test.Services
{
    public class ChatService
    {
        private readonly IHubContext<ChatServiceHub> chatServiceHub;

        // 用于存储用户连接ID与用户名的映射
        private static readonly ConcurrentDictionary<string, string> _connections = [];

        // 用于存储用户名与连接ID的映射
        private static readonly ConcurrentDictionary<string, string> _users = [];

        public ChatService(IHubContext<ChatServiceHub> hubContext)
        {
            chatServiceHub = hubContext;
        }

        public async Task SendToMe(string connectionId, string message)
        {
            if (!_connections.ContainsKey(connectionId))
            {
                await chatServiceHub.Clients.Client(connectionId).SendAsync("ErrorMessage", "You must be signed in to send messages.");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                await chatServiceHub.Clients.Client(connectionId).SendAsync("ErrorMessage", "Message cannot be empty.");
                return;
            }

            await chatServiceHub.Clients.Client(connectionId).SendAsync("ReceiveMessage", $"{DateTime.Now}", _connections[connectionId], message, false);
        }

        public async Task SendToAll(string connectionId, string message)
        {
            if (!_connections.ContainsKey(connectionId))
            {
                await chatServiceHub.Clients.Client(connectionId).SendAsync("ErrorMessage", "You must be signed in to send messages.");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                await chatServiceHub.Clients.Client(connectionId).SendAsync("ErrorMessage", "Message cannot be empty.");
                return;
            }

            await chatServiceHub.Clients.All.SendAsync("ReceiveMessage", $"{DateTime.Now}", _connections[connectionId], message, false);
        }

        public async Task SendToUser(string connectionId, string username, string message)
        {
            if (!_connections.ContainsKey(connectionId))
            {
                await chatServiceHub.Clients.Client(connectionId).SendAsync("ErrorMessage", "You must be signed in to send messages.");
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                await chatServiceHub.Clients.Client(connectionId).SendAsync("ErrorMessage", "Username cannot be empty.");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                await chatServiceHub.Clients.Client(connectionId).SendAsync("ErrorMessage", "Message cannot be empty.");
                return;
            }

            if (_users.TryGetValue(username, out var userConnectionId))
            {
                await chatServiceHub.Clients.Client(userConnectionId).SendAsync("ReceiveMessage", $"{DateTime.Now}", _connections[connectionId], message, true);
            }
            else
            {
                await chatServiceHub.Clients.Client(connectionId).SendAsync("ErrorMessage", $"User '{username}' does not exist.");
            }
        }

        public async Task UserSignin(string connectionId, string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                await chatServiceHub.Clients.Client(connectionId).SendAsync("ErrorMessage", "Username cannot be empty.");
                return;
            }

            if (_users.ContainsKey(username))
            {
                await chatServiceHub.Clients.Client(connectionId).SendAsync("ErrorMessage", "Username already exists.");
                return;
            }

            _connections[connectionId] = username;
            _users[username] = connectionId;

            await chatServiceHub.Clients.All.SendAsync("UserSignedIn", username);
        }

        public async Task UserSignout(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return;
            }

            if (_users.TryRemove(username, out var connectionId))
            {
                _connections.TryRemove(connectionId, out _);
                await chatServiceHub.Clients.All.SendAsync("UserSignedOut", username);
            }
        }

        public async Task HandleDisconnect(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var username))
            {
                _users.TryRemove(username, out _);
                await chatServiceHub.Clients.All.SendAsync("UserSignedOut", username);
            }
        }
    }
}