using MbtxAssessment.Data;
using MbtxAssessment.Models;
using Microsoft.AspNetCore.SignalR;

namespace MbtxAssessment.Services;

public class SensorService
{
    private readonly SensorReadingStore _sensorReadingStore;
    private readonly AnomalyStore _anomalyStore;
    private readonly ILogger<SensorService> _logger;
    private readonly IHubContext<SensorHub> _hubContext;
    private readonly AnomalyDetector _anomalyDetectorTemperature = new("Temperature");
    private readonly AnomalyDetector _anomalyDetectorHumidity = new("Humidity");
    private readonly AnomalyDetector _anomalyDetectorCo2Ppm = new("Co2Ppm");

    public SensorService(SensorReadingStore sensorReadingStore, AnomalyStore anomalyStore, ILogger<SensorService> logger, IHubContext<SensorHub> hubContext)
    {
        _sensorReadingStore = sensorReadingStore;
        _anomalyStore = anomalyStore;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task ProcessNewReadings(IEnumerable<SensorReading> readings)
    {
        if (readings == null || !readings.Any())
        {
            _logger.LogWarning("No sensor readings to process.");
            return;
        }

        foreach (var reading in readings)
        {
            if (reading.Id == Guid.Empty)
            {
                reading.Id = Guid.NewGuid();
            }
        }

        _sensorReadingStore.SaveAll(readings);

        foreach (var reading in readings)
        {
            await _hubContext.Clients.All.SendAsync("sensorReadingAvailable", reading);
            _logger.LogInformation($"Broadcasted new sensor reading: Id={reading.Id}, SequenceNumber={reading.SequenceNumber}, SensorId={reading.SensorId}, Timestamp={reading.Timestamp}");
            await ProcessAnomalies(reading);
        }
    }

    public SensorReadingEntity? GetLatestReading()
    {
        return _sensorReadingStore.GetLatest();
    }

    private async Task ProcessAnomalies(SensorReading reading)
    {
        var anomalies = new List<Anomaly?>
        {
            _anomalyDetectorTemperature.Analyze(reading.Temperature),
            _anomalyDetectorHumidity.Analyze(reading.Humidity),
            _anomalyDetectorCo2Ppm.Analyze(reading.Co2Ppm)
        }.Where(a => a != null).ToList();

        foreach (var anomaly in anomalies)
        {
            _logger.LogWarning($"Anomaly detected: {anomaly?.Reason}");
            await _hubContext.Clients.All.SendAsync("anomalyDetected", anomaly);
            _anomalyStore.Save(anomaly);
        }
    }
}