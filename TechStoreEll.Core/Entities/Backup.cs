using System.Text.Json.Serialization;

namespace TechStoreEll.Core.Entities;

public partial class Backup : IEntity
{
    public int Id { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Filename { get; set; }

    public string? Command { get; set; }

    public string? Note { get; set; }
    
    [JsonIgnore]
    public virtual User? CreatedByNavigation { get; set; }
}
