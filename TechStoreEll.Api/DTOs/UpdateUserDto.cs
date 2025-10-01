namespace TechStoreEll.Api.DTOs;

public class UpdateUserDto
{
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? MiddleName { get; set; }
}

public class UpdateUserSettingsDto
{
    public string? Theme { get; set; }
    public int? ItemsPerPage { get; set; }
    public string? DateFormat { get; set; }
    public string? NumberFormat { get; set; }
    public string? SavedFilters { get; set; }
    public string? Hotkeys { get; set; }
}