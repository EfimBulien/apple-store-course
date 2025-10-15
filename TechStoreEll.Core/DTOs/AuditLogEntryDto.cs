namespace TechStoreEll.Core.DTOs;

public class AuditLogEntryDto
{
    public long Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public char Operation { get; set; }
    public string? RecordId { get; set; }
    public long? ChangedByUserId { get; set; }
    public string? ChangedByUsername { get; set; } // для отображения имени
    public DateTime ChangedAt { get; set; }
    public Dictionary<string, object>? OldRow { get; set; }
    public Dictionary<string, object>? NewRow { get; set; }
}