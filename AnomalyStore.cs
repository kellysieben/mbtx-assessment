using Microsoft.EntityFrameworkCore;

namespace MbtxAssessment;

public class AnomalyStore(IDbContextFactory<AppDbContext> dbContextFactory)
{
    public void Save(Anomaly anomaly)
    {
        if (anomaly == null)
        {
            return;
        }

        using var db = dbContextFactory.CreateDbContext();
        db.Anomalies.Add(new AnomalyEntity
        {
            Id = anomaly.Id,
            DetectedAt = anomaly.DetectedAt,
            SensorType = anomaly.SensorType,
            Value = anomaly.Value,
            ZScore = anomaly.ZScore,
            Reason = anomaly.Reason
        });
        db.SaveChanges();
    }
}
