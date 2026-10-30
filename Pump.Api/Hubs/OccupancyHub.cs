using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;

namespace Pump.Api.Hubs;

[Authorize]
public class OccupancyHub : Hub
{
    public const string AdminsGroup = "admins";
    public static string UserGroup(int userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole("Admin") == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);

        var userIdValue = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (int.TryParse(userIdValue, out var userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

        await base.OnConnectedAsync();
    }
}
