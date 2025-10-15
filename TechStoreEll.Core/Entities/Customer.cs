namespace TechStoreEll.Core.Entities;

public partial class Customer : IEntity
{
    // это внешняя зависимость к UserId = ID
    public int Id { get; set; } // равносильно public int UserId

    public int? ShippingAddressId { get; set; }

    public int? BillingAddressId { get; set; }

    public int? LoyaltyPoints { get; set; }

    public virtual Address? BillingAddress { get; set; }

    public virtual Address? ShippingAddress { get; set; }

    public virtual User User { get; set; } = null!;
}
