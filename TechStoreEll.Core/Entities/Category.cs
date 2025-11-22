using System.Text.Json.Serialization;

namespace TechStoreEll.Core.Entities;

public partial class Category : IEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ParentId { get; set; }

    public string? Description { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    [JsonIgnore]
    public virtual Category? Parent { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
