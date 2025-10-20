namespace TechStoreEll.Web.Models;

public class ProductListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Sku { get; set; }
    public string CategoryName { get; set; }
    public int VariantCount { get; set; }
    public bool Active { get; set; }
}
