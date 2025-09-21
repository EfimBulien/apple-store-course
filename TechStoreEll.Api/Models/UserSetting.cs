using System;
using System.Collections.Generic;

namespace TechStoreEll.Api.Models;

public partial class UserSetting
{
    public long UserId { get; set; }

    public string? Theme { get; set; }

    public int? ItemsPerPage { get; set; }

    public string? DateFormat { get; set; }

    public string? NumberFormat { get; set; }

    public string? SavedFilters { get; set; }

    public string? Hotkeys { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
