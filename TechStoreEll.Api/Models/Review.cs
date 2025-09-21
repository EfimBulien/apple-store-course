using System;
using System.Collections.Generic;

namespace TechStoreEll.Api.Models;

public partial class Review
{
    public long Id { get; set; }

    public int ProductId { get; set; }

    public long? UserId { get; set; }

    public short Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsModerated { get; set; }

    public long? ModeratedBy { get; set; }

    public virtual User? ModeratedByNavigation { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User? User { get; set; }
}
