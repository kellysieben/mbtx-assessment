using System.ComponentModel.DataAnnotations;

namespace MbtxAssessment.Data;

public class SensorReadingEntity
{
    [Key]
    public Guid Id { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal Temperature { get; set; }
    public decimal Humidity { get; set; }
    public int Co2Ppm { get; set; }
}
