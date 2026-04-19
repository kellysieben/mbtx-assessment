using MbtxAssessment.SensorReadings;
using Microsoft.AspNetCore.SignalR;

namespace MbtxAssessment;

public class SensorService
{
    private readonly SensorReadingStore _store;
    private readonly ILogger<SensorService> _logger;
    private readonly IHubContext<SensorHub> _hubContext;

    public SensorService(SensorReadingStore store, ILogger<SensorService> logger, IHubContext<SensorHub> hubContext)
    {
        _store = store;
        _logger = logger;
        _hubContext = hubContext;
    }

// This method processes new sensor readings by saving them to the database and broadcasting them to connected clients via SignalR. 
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

        _store.SaveAll(readings);

        foreach (var reading in readings)
        {
            await _hubContext.Clients.All.SendAsync("sensorReadingAvailable", reading);
            _logger.LogInformation($"Broadcasted new sensor reading: Id={reading.Id}, SequenceNumber={reading.SequenceNumber}, SensorId={reading.SensorId}, Timestamp={reading.Timestamp}");
        }
    }

    public SensorReadingEntity? GetLatestReading()
    {
        return _store.GetLatest();
    }
}