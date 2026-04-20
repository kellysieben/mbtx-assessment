namespace MbtxAssessment;

public record SensorReading
{
    public Guid Id { get; set; }
    public string SensorId { get; init; } = string.Empty;
    public long SequenceNumber { get; init; }
    public DateTime Timestamp { get; init; }
    public decimal Temperature { get; init; }
    public decimal Humidity { get; init; }
    public int Co2Ppm { get; init; }
}