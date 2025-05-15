using Microsoft.AspNetCore.SignalR;

namespace NEXT_BMS.Models
{
    
    public class ChatHub : Hub
    {
       
            public async Task SendMessage(int receiverId, string message)
            {
            await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", message);
         
        }
       


        public override Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier;
            Console.WriteLine($"User connected: {userId}");
            return base.OnConnectedAsync();
        }
    }

}

