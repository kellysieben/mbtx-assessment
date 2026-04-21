using MbtxAssessment.Services;

namespace MbtxAssessment.Tests;

public class AnomalyDetectorTests
{
    [Fact]
    public void AnomalyDetector_WithLessThan5Readings_ReturnsNoAnomaly()
    {
        AnomalyDetector detector = new("test-x");
        detector.Analyze(22.1m);
        detector.Analyze(21.9m);
        detector.Analyze(22.3m);
        var anomaly = detector.Analyze(87.0m);
        Assert.Null(anomaly);
    }

    [Fact]
    public void AnomalyDetector_WithSpike_ReturnsAnomaly()
    {
        decimal[] baseline =
        [
            22.1m, 21.9m, 22.3m, 22.0m, 21.8m,
            22.2m, 22.0m, 21.7m, 22.4m, 22.1m,
            21.9m, 22.3m, 22.0m, 21.8m, 22.2m,
        ];

        AnomalyDetector detector = new("test-x");

        foreach (var value in baseline)
            detector.Analyze(value);

        // Spike
        var anomaly = detector.Analyze(87.0m);

        Assert.NotNull(anomaly);
        Assert.Equal("test-x", anomaly.SensorType);
        Assert.Equal(87.0m, anomaly.Value);
        Assert.True(anomaly.ZScore > 2.5m, $"Expected z-score > 2.5, got {anomaly.ZScore}");
    }
}
