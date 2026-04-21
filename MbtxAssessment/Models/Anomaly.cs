namespace MbtxAssessment.Models;

public record Anomaly
{
    public Guid Id { get; init; }
    public DateTime DetectedAt { get; init; }
    public string SensorType { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public decimal ZScore { get; init; }
    public string Reason { get; init; } = string.Empty;
}
