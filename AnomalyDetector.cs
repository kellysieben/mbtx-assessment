namespace MbtxAssessment;

public class AnomalyDetector
{
    private readonly string _sensorType;
    private readonly Queue<decimal> _recentValues = new(20);
    private const int FifoCapacity = 20;

    public AnomalyDetector(string sensorType)
    {
        _sensorType = sensorType;
    }

    public Anomaly? Analyze(decimal newValue)
    {
        if (_recentValues.Count == FifoCapacity)
            _recentValues.Dequeue();
        _recentValues.Enqueue(newValue);

        if (_recentValues.Count < 5) // Need at least 5 readings to analyze
            return null;

        decimal mean = _recentValues.Average();
        decimal stddev = (decimal)Math.Sqrt(_recentValues.Average(v => Math.Pow((double)(v - mean), 2)));
        decimal zScore = stddev == 0 ? 0 : (newValue - mean) / stddev;

        if (Math.Abs(zScore) > 1.5m)
        {
            return new Anomaly
            {
                Id = Guid.NewGuid(),
                DetectedAt = DateTime.UtcNow,
                SensorType = _sensorType,
                Value = newValue,
                ZScore = zScore,
                Reason = $"Value [{newValue}] triggered an anomaly with Z-Score [{zScore:F2}]"
            };
        }

        return null;
    }
}