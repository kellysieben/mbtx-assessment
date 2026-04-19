using MbtxAssessment;
using MbtxAssessment.SensorReadings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=mbtx.db"));

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddSignalR();
builder.Services.AddSingleton<SensorReadingStore>();
builder.Services.AddHostedService<TestSensorService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();

app.UseDefaultFiles();
app.UseStaticFiles();

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

    foreach (var reading in readings)    {
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

app.Run();
