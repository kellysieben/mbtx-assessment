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

    public List<Anomaly> GetAll(int? limit = null)
    {
        using var db = dbContextFactory.CreateDbContext();
        IQueryable<AnomalyEntity> query = db.Anomalies
            .AsNoTracking()
            .OrderByDescending(a => a.DetectedAt);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return query
            .Select(a => new Anomaly
            {
                Id = a.Id,
                DetectedAt = a.DetectedAt,
                SensorType = a.SensorType,
                Value = a.Value,
                ZScore = a.ZScore,
                Reason = a.Reason
            })
            .ToList();
    }
}
