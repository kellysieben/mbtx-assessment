using System.ComponentModel.DataAnnotations;

namespace MbtxAssessment.DataStore;

public class RegisteredClientEntity
{
    [Key]
    [MaxLength(256)]
    public string ClientId { get; set; } = string.Empty;

    public DateTime RegisteredAt { get; set; }
}
