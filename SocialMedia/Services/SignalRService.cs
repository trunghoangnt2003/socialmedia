using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SocialMedia.Services
{
    public class SignalRService : Hub
    {
        public static ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null)
            {
                Debug.WriteLine("HttpContext is null.");
                await base.OnConnectedAsync();
                return;
            }

            var session = httpContext.Session;
            var oldConnectionId = session.GetString("ConnectionId");

            if (!string.IsNullOrEmpty(oldConnectionId))
            {
                var key = ConnectedUsers.FirstOrDefault(x => x.Value == oldConnectionId).Key;
                if (!string.IsNullOrEmpty(key))
                {
                    ConnectedUsers[key] = Context.ConnectionId;
                }
            }

            session.SetString("ConnectionId", Context.ConnectionId);
            var user = session.GetString("User");

            if (!string.IsNullOrEmpty(user))
            {
                ConnectedUsers.AddOrUpdate(user, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
                Debug.WriteLine($"User {user} connected with ConnectionId {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext == null)
            {
                Debug.WriteLine("HttpContext is null.");
                await base.OnDisconnectedAsync(exception);
                return;
            }

            var session = httpContext.Session;
            var currentConnectionId = session.GetString("ConnectionId");

            if (!string.IsNullOrEmpty(currentConnectionId))
            {
                var key = ConnectedUsers.FirstOrDefault(x => x.Value == currentConnectionId).Key;
                if (!string.IsNullOrEmpty(key))
                {
                    ConnectedUsers.TryRemove(key, out _);
                    Debug.WriteLine($"User {key} disconnected.");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string sender, string receiver)
        {
            if (ConnectedUsers.TryGetValue(receiver, out string receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessageFrom", sender);
            }
            if (ConnectedUsers.TryGetValue(sender, out string senderConnectionId))
            {
                await Clients.Client(senderConnectionId).SendAsync("SendMessageTo", receiver);
            }
        }


        public async Task SendNotification(string senderName, string receiverId, string message, string timestamp, string postId,string notiId)
        {
            if (ConnectedUsers.TryGetValue(receiverId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveNotification", senderName, message, timestamp,postId,notiId);
            }
        }
    }
}