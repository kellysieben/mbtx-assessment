namespace MbtxAssessment.SensorReadings;

public class TestSensorReadingGenerator(
    ILogger<TestSensorReadingGenerator> logger) : BackgroundService
{
    private readonly Random _random = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            SensorReading reading = GenerateRandomReading();

            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.PostAsJsonAsync("http://localhost:5241/api/readings", new[] { reading }, stoppingToken);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"THUMP! Failed to post sensor reading: {response.StatusCode}");
                }
                else
                {
                    logger.LogInformation($"THUMP! Sensor Reading: .[{reading.SequenceNumber}]  .[{reading.SensorId}]  .[{reading.Timestamp}]");
                }
            }
            catch (Exception ex)
            {
                // for now, just eat the exception and log it - in a real app, we might want to implement retry logic or other error handling
                logger.LogError(ex, "THUMP! Exception while posting sensor reading");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private SensorReading GenerateRandomReading()
    {
        var reading = new SensorReading
        {
            // done on server - Id = Guid.NewGuid(),
            SensorId = "sensor-test-thump",
            SequenceNumber = DateTime.UtcNow.Ticks,
            Timestamp = DateTime.UtcNow,
            Temperature = (decimal)(15 + _random.NextDouble() * 15),
            Humidity = (decimal)(_random.NextDouble() * 100),
            Co2Ppm = (int)(_random.NextDouble() * 1000)
        };
        return reading;
    }
}