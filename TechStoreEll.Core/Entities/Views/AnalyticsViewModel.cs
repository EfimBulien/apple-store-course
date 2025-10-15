namespace TechStoreEll.Core.Entities.Views;

public class AnalyticsViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<SalesByDay> SalesByDay { get; set; } = new();

    public List<TopCategory> TopCategories { get; set; } = new();

    public List<TopProduct> TopProducts { get; set; } = new();

    public List<UserActivity> UserActivities { get; set; } = new();
}

public class SalesByDay
{
    public DateOnly Date { get; set; }
    public decimal Total { get; set; }
}

public class TopCategory
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}

public class TopProduct
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
}

public class UserActivity
{
    public string FullName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
}