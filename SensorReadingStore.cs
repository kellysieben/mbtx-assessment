using Microsoft.EntityFrameworkCore;

namespace MbtxAssessment;

public class SensorReadingStore(IDbContextFactory<AppDbContext> dbContextFactory)
{
    public void Save(SensorReading reading)
    {
        using var db = dbContextFactory.CreateDbContext();
        db.SensorReadings.Add(new SensorReadingEntity
        {
            Id = reading.Id,
            SensorId = reading.SensorId,
            SequenceNumber = reading.SequenceNumber,
            Timestamp = reading.Timestamp,
            Temperature = reading.Temperature,
            Humidity = reading.Humidity,
            Co2Ppm = reading.Co2Ppm,
            IsValid = reading.IsValid
        });
        db.SaveChanges();
    }
}
