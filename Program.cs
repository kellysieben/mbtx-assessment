using MbtxAssessment.DataStore;
using MbtxAssessment.SensorReadings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<IRegisteredClientStore, RegisteredClientStore>();
builder.Services.AddHostedService<TestSensorService>();

var app = builder.Build();

app.MapGet("/api/happy", () => ":-)");

app.MapHub<SensorHub>("/hubs/sensor");

app.Run();
