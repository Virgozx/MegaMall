using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MegaMall.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task SendMessageToAdmin(string message)
        {
            var user = Context.User.Identity.Name;
            await Clients.Group("Admins").SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMessageToUser(string userId, string message)
        {
            var adminName = "Support";
            await Clients.User(userId).SendAsync("ReceiveMessage", adminName, message);
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User.IsInRole("Admin"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            await base.OnConnectedAsync();
        }
    }
}
