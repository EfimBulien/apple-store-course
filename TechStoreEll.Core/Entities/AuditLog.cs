namespace TechStoreEll.Core.Entities;

public partial class AuditLog : IEntity
{
    public int Id { get; set; }

    public string TableName { get; set; } = null!;

    public char Operation { get; set; }

    public string? RecordId { get; set; }

    public int? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? OldRow { get; set; }

    public string? NewRow { get; set; }

    public virtual User? ChangedByNavigation { get; set; }
}
