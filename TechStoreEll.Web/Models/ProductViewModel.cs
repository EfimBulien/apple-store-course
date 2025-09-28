namespace TechStoreEll.Web.Models;

public class ProductViewModel
{
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public string CategoryName { get; set; }
    public decimal? Price { get; set; }
    public string PrimaryImageUrl { get; set; }
    public string PrimaryImageAltText { get; set; }
    public decimal? AvgRating { get; set; }
    public int ReviewsCount { get; set; }
    
    public DateTime CreatedAt { get; set; }
}