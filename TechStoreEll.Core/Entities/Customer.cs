using System.Text.Json.Serialization;

namespace TechStoreEll.Core.Entities;

public partial class Customer : IEntity
{
    // это внешняя зависимость к UserId = ID
    public int Id { get; set; } // равносильно public int UserId

    public int? ShippingAddressId { get; set; }

    public int? BillingAddressId { get; set; }

    [JsonIgnore]
    public int? LoyaltyPoints { get; set; }

    [JsonIgnore]
    public virtual Address? BillingAddress { get; set; }

    [JsonIgnore]
    public virtual Address? ShippingAddress { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; } = null!;
}
