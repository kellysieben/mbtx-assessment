using Microsoft.AspNetCore.SignalR;
using MbtxAssessment.DataStore;

namespace MbtxAssessment.SensorReadings;

public class SensorHub(IRegisteredClientStore store) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var clientId = Context.GetHttpContext()?.Request.Query["clientId"].ToString();

        if (!string.IsNullOrWhiteSpace(clientId) && store.IsRegistered(clientId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, clientId);
        }

        await base.OnConnectedAsync();
    }
}
