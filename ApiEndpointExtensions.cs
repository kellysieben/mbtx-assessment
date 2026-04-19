using MbtxAssessment.SensorReadings;
using Microsoft.AspNetCore.SignalR;

namespace MbtxAssessment;

public static class ApiEndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/api/happy", () => ":-)");

        app.MapGet("/api/readings/latest", (SensorReadingStore store) =>
        {
            var latest = store.GetLatest();
            return latest is null ? Results.NotFound() : Results.Ok(latest);
        });

        app.MapPost("/api/readings", async (
            SensorReading[] readings,
            SensorReadingStore store,
            IHubContext<SensorHub> hubContext) =>
        {
            if (readings.Length == 0)
                return Results.BadRequest("Readings array must not be empty.");

            foreach (var reading in readings)
            {
                if (reading.Id == Guid.Empty)
                {
                    reading.Id = Guid.NewGuid();
                }
            }

            store.SaveAll(readings);

            foreach (var reading in readings)
            {
                await hubContext.Clients.All.SendAsync("sensorReadingAvailable", reading);
                app.Logger.LogInformation($"POST .BROADCAST .ALL  .[{reading.Id}] .[{reading.SequenceNumber}]  .[{reading.SensorId}]  .[{reading.Timestamp}]");
            }

            return Results.Ok();
        });

        app.MapHub<SensorHub>("/hubs/sensor");

        app.MapFallbackToFile("index.html");

        return app;
    }
}
