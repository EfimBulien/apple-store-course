using System.Text.Json.Serialization;

namespace TechStoreEll.Core.Entities;

public partial class Role : IEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
