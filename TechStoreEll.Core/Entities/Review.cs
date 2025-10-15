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

    public virtual User? ModeratedByNavigation { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User? User { get; set; }
}
