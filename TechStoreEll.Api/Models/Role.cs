using TechStoreEll.Api.Entities;

namespace TechStoreEll.Api.Models;

public partial class Role : IEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
