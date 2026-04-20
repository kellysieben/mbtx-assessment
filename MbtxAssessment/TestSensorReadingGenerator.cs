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
            Temperature = (decimal)GenerateValue(baseline: 22, noise: 2.0, spikeChance: 0.07, spikeMin: 9, spikeMax: 13),
            Humidity    = (decimal)GenerateValue(baseline: 60, noise: 5.0, spikeChance: 0.07, spikeMin: 25, spikeMax: 35),
            Co2Ppm      = (int)   GenerateValue(baseline: 600, noise: 80,  spikeChance: 0.07, spikeMin: 400, spikeMax: 600),
        };
        return reading;
    }

    private double GenerateValue(double baseline, double noise, double spikeChance, double spikeMin, double spikeMax)
    {
        double value = baseline + (_random.NextDouble() * 2 - 1) * noise;
        if (_random.NextDouble() < spikeChance)
        {
            double spike = spikeMin + _random.NextDouble() * (spikeMax - spikeMin);
            value += _random.NextDouble() < 0.5 ? spike : -spike;
        }
        return value;
    }
}