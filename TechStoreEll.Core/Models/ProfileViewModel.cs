using TechStoreEll.Core.DTOs;

namespace TechStoreEll.Core.Models;

public class ProfileViewModel
{
    // ничего не трогать
    public UpdateUserDto User { get; set; }
    public UpdateUserSettingsDto Settings { get; set; } = new();
}