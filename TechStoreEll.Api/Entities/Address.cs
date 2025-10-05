﻿namespace TechStoreEll.Api.Entities;

public partial class Address : IEntity
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? Label { get; set; }

    public string Country { get; set; } = null!;

    public string? Region { get; set; }

    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public string House { get; set; } = null!;

    public string? Apartment { get; set; }

    public string? Postcode { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Customer> CustomerBillingAddresses { get; set; } = new List<Customer>();

    public virtual ICollection<Customer> CustomerShippingAddresses { get; set; } = new List<Customer>();

    public virtual User? User { get; set; }
}
