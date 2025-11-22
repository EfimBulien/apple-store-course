using System.Text.Json.Serialization;

namespace TechStoreEll.Core.Entities;

public partial class Review : IEntity
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int? UserId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsModerated { get; set; }

    public int? ModeratedBy { get; set; }

    [JsonIgnore]
    public virtual User? ModeratedByNavigation { get; set; }

    [JsonIgnore]
    public virtual Product Product { get; set; } = null!;

    [JsonIgnore]
    public virtual User? User { get; set; }
}
