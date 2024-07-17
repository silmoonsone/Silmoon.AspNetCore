using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Silmoon.AspNetCore.Test.Hubs
{
    public class ChatHub : Hub
    {
        // 用于存储用户连接ID与用户名的映射
        private static readonly ConcurrentDictionary<string, string> _connections = new ConcurrentDictionary<string, string>();

        // 用于存储用户名与连接ID的映射
        private static readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();

        public async Task SendToMe(string message)
        {
            if (!_connections.ContainsKey(Context.ConnectionId))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "You must be signed in to send messages.");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Message cannot be empty.");
                return;
            }
            var client = Clients.Caller;
            await client.SendAsync("ReceiveMessage", $"{DateTime.Now}", _connections[Context.ConnectionId], message, false);
        }

        public async Task SendToAll(string message)
        {
            if (!_connections.ContainsKey(Context.ConnectionId))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "You must be signed in to send messages.");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Message cannot be empty.");
                return;
            }
            await Clients.All.SendAsync("ReceiveMessage", $"{DateTime.Now}", _connections[Context.ConnectionId], message, false);
        }

        public async Task SendToUser(string username, string message)
        {
            if (!_connections.ContainsKey(Context.ConnectionId))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "You must be signed in to send messages.");
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Username cannot be empty.");
                return;
            }
            if (string.IsNullOrEmpty(message))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Message cannot be empty.");
                return;
            }
            var connectionId = _users.FirstOrDefault(x => x.Key == username).Value;
            if (!string.IsNullOrEmpty(connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", $"{DateTime.Now}", _connections[Context.ConnectionId], message, true);
            }
            else
            {
                await Clients.Caller.SendAsync("ErrorMessage", $"User '{username}' does not exist.");
            }
        }

        public async Task UserSignin(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Username cannot be empty.");
                return;
            }
            if (_users.ContainsKey(username))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Username already exists.");
                return;
            }
            _connections[Context.ConnectionId] = username;
            _users[username] = Context.ConnectionId;
            await Clients.All.SendAsync("UserSignedIn", username);
        }

        public async Task UserSignout(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Username cannot be empty.");
                return;
            }
            var connectionId = _users.FirstOrDefault(x => x.Key == username).Value;
            if (!string.IsNullOrEmpty(connectionId))
            {
                _connections.TryRemove(connectionId, out _);
                _users.TryRemove(username, out _);
                await Clients.All.SendAsync("UserSignedOut", username);
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryRemove(Context.ConnectionId, out var username))
            {
                _users.TryRemove(username, out _);
                Clients.All.SendAsync("UserSignedOut", username);
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
