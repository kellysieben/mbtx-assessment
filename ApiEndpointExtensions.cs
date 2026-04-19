using MbtxAssessment.SensorReadings;

namespace MbtxAssessment;

public static class ApiEndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/api/happy", () => ":-)");

        app.MapGet("/api/readings/latest", (SensorService service) =>
        {
            var latest = service.GetLatestReading();
            return latest is null ? Results.NotFound() : Results.Ok(latest);
        });

        app.MapPost("/api/readings", async (
            SensorReading[] readings,
            SensorService service) =>
        {
            await service.ProcessNewReadings(readings);
            return Results.Ok();  // Results.BadRequest("Readings array must not be empty.");
        });

        app.MapHub<SensorHub>("/hubs/sensor");

        app.MapFallbackToFile("index.html");

        return app;
    }
}
