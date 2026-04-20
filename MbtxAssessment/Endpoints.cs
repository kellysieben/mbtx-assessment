using MbtxAssessment.SensorReadings;

namespace MbtxAssessment;

public static class Endpoints
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapGet("/api/happy", () => ":-)");

        app.MapGet("/api/readings/latest", (SensorService service) =>
        {
            var latest = service.GetLatestReading();
            return latest is null ? Results.NotFound() : Results.Ok(latest);
        });

        app.MapGet("/api/anomaly", (AnomalyStore store, int? limit) =>
        {
            var anomalies = store.GetAll(limit);
            return Results.Ok(anomalies);
        });

        app.MapPost("/api/readings", async (
            SensorReading[] readings,
            SensorService service) =>
        {
            await service.ProcessNewReadings(readings);
            return Results.Ok();  // todo: error checking on service call ... Results.BadRequest("reason why it failed");
        });

        app.MapHub<SensorHub>("/hubs/sensor");

        app.MapFallbackToFile("index.html");

        return app;
    }
}
