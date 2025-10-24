namespace TechStoreEll.Web.Models;

public class DatabaseErrorViewModel
{
    public string Message { get; set; } = "Не удалось подключиться к базе данных.";
    public string DetailedError { get; set; } = string.Empty;
}