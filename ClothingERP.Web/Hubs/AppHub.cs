using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ClothingERP.Web.Hubs;

[Authorize] 
public class AppHub : Hub
{
   

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}