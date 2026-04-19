using MbtxAssessment;
using MbtxAssessment.DataStore;
using MbtxAssessment.SensorReadings;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=mbtx.db"));

builder.Services.AddSignalR();
builder.Services.AddSingleton<IRegisteredClientStore, RegisteredClientStore>();
builder.Services.AddHostedService<TestSensorService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/happy", () => ":-)");

app.MapPost("/clients/register", (string clientId, IRegisteredClientStore store) =>
{
    // validate clientId
    if (string.IsNullOrWhiteSpace(clientId))    {
        return Results.BadRequest("Client ID cannot be null or empty.");
    }
    var added = store.RegisterClient(clientId);
    return added ? Results.Ok() : Results.Conflict($"Client '{clientId}' is already registered.");
});

app.MapHub<SensorHub>("/hubs/sensor");

app.MapFallbackToFile("index.html");

app.Run();
