public record SensorReading
{
    public Guid Id { get; init; }
    public long SequenceNumber { get; init; }
    public DateTime Timestamp { get; init; }
    public decimal Temperature { get; init; }
    public decimal Humidity { get; init; }
    public int Co2Ppm { get; init; }
}