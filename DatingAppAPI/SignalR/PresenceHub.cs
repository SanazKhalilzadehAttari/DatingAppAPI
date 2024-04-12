using DatingAppAPI.Extnsions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DatingAppAPI.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
           var isOnline =  await _tracker.UserConnected(Context.User.getUser(), Context.ConnectionId);
            if(isOnline)
            await Clients.Others.SendAsync("user is online", Context.User.getUser());
            var currentUsers = await _tracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await _tracker.UserDisconnected(Context.User.getUser(), Context.ConnectionId);
            if( isOffline)
            await Clients.Others.SendAsync("User is offline", Context.User.getUser());

            await  base.OnDisconnectedAsync(exception);
        }
    }
}
