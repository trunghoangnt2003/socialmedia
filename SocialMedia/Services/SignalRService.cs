using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SocialMedia.Services
{
    public class SignalRService : Hub
    {
        public static ConcurrentDictionary<string, string> connectedUser = new ConcurrentDictionary<string, string>();

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
                var key = connectedUser.FirstOrDefault(x => x.Value == oldConnectionId).Key;
                if (!string.IsNullOrEmpty(key))
                {
                    connectedUser[key] = Context.ConnectionId;
                }
            }

            session.SetString("ConnectionId", Context.ConnectionId);

            var user = session.GetString("User");

            if (!string.IsNullOrEmpty(user))
            {
                connectedUser.AddOrUpdate(user, Context.ConnectionId, (key, oldvalue) => Context.ConnectionId);
                Debug.WriteLine($"User {user} connected with ConnectionId {Context.ConnectionId}");
            }
            else
            {
                Debug.WriteLine("User is null or empty.");
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
                var key = connectedUser.FirstOrDefault(x => x.Value == currentConnectionId).Key;
                if (!string.IsNullOrEmpty(key))
                {
                    connectedUser.TryRemove(key, out _);
                    Debug.WriteLine($"User {key} disconnected.");
                }
            }

            Debug.WriteLine("Disconnected: " + Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
