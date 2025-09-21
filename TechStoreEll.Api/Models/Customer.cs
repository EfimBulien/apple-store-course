using System;
using System.Collections.Generic;

namespace TechStoreEll.Api.Models;

public partial class Customer
{
    public long UserId { get; set; }

    public long? ShippingAddressId { get; set; }

    public long? BillingAddressId { get; set; }

    public int? LoyaltyPoints { get; set; }

    public virtual Address? BillingAddress { get; set; }

    public virtual Address? ShippingAddress { get; set; }

    public virtual User User { get; set; } = null!;
}
