namespace TechStoreEll.Core.Entities;

public partial class UserSetting : IEntity
{
    // это внешняя зависимость к UserId = ID
    public int Id { get; set; }

    public string? Theme { get; set; }

    public int? ItemsPerPage { get; set; }

    public string? DateFormat { get; set; }

    public string? NumberFormat { get; set; }

    public string? SavedFilters { get; set; }

    public string? Hotkeys { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
