using TechStoreEll.Core.DTOs;

namespace TechStoreEll.Core.Models;

public class ProfileViewModel
{
    // ничего не трогать
    public UpdateUserDto User { get; set; }
    public UpdateSettingsDto Settings { get; set; } = new();
}