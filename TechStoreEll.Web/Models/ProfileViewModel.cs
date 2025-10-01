using TechStoreEll.Api.DTOs;

namespace TechStoreEll.Web.Models;

public class ProfileViewModel
{
    // ничего не трогать
    public UpdateUserDto User { get; set; }
    public UpdateUserSettingsDto Settings { get; set; } = new();
}