using System;
using System.Collections.Generic;

namespace TechStoreEll.Api.Models;

public partial class Backup
{
    public long Id { get; set; }

    public long? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Filename { get; set; }

    public string? Command { get; set; }

    public string? Note { get; set; }

    public virtual User? CreatedByNavigation { get; set; }
}
