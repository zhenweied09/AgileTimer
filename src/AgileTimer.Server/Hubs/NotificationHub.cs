using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AgileTimer.Server.Hubs
{
    public class NotificationHub:Hub
    {
        public async Task SendToggleTimerMessage(string user)
        {
            await Clients.All.SendAsync("ReceivedToggleTimerMessage");
        }
    }
}
