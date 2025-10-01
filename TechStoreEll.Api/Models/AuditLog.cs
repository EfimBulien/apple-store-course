using System;
using System.Collections.Generic;

namespace TechStoreEll.Api.Models;

public partial class AuditLog
{
    public long Id { get; set; }

    public string TableName { get; set; } = null!;

    public char Operation { get; set; }

    public string? RecordId { get; set; }

    public long? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? OldRow { get; set; }

    public string? NewRow { get; set; }

    public virtual User? ChangedByNavigation { get; set; }
}
