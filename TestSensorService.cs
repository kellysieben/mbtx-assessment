using Microsoft.AspNetCore.SignalR;
using MbtxAssessment.DataStore;

namespace MbtxAssessment.SensorReadings;

public class TestSensorService(
    IHubContext<SensorHub> hubContext,
    IRegisteredClientStore clientStore,
    SensorReadingStore sensorReadingStore,
    ILogger<TestSensorService> logger) : BackgroundService
{
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            SensorReading reading = GenerateRandomReading();

            if (!reading.IsValid)
            {
                logger.LogWarning($"Invalid sensor reading: {reading}");
                continue;
            }

            var registeredClients = clientStore.GetRegisteredClients();

            if (registeredClients.Count > 0)
            {

                await hubContext.Clients
                    .Groups(registeredClients.ToList())
                    .SendAsync("sensorReadingAvailable", reading, stoppingToken);

            }

            logger.LogInformation($"BROADCAST: {reading} ==> [ {registeredClients.Count} ] registered client group(s).");
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private SensorReading GenerateRandomReading()
    {
        var reading = new SensorReading
        {
            Id = Guid.NewGuid(),
            SensorId = "sensor-test-001",
            SequenceNumber = DateTime.UtcNow.Ticks,
            Timestamp = DateTime.UtcNow,
            Temperature = (decimal)(15 + _random.NextDouble() * 15),
            Humidity = (decimal)(_random.NextDouble() * 100),
            Co2Ppm = (int)(_random.NextDouble() * 1000)
        };
        sensorReadingStore.Save(reading);
        return reading;
    }
}