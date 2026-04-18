using MbtxAssessment.DataStore;
using MbtxAssessment.SensorReadings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<IRegisteredClientStore, RegisteredClientStore>();
builder.Services.AddHostedService<TestSensorService>();

var app = builder.Build();

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

app.Run();
