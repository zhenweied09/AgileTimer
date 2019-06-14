using System.Threading.Tasks;
using AgileTimer.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AgileTimer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimerController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public TimerController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpGet("toggle")]
        public async Task<IActionResult> StartTimer()
        {
            await _hubContext.Clients.All.SendAsync("ReceivedToggleTimerMessage", nameof(TimerController));
            return NoContent();
        }
    }
}
