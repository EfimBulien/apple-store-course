namespace TechStoreEll.Web.Models;

public class CheckoutModel
{
    public int? ShippingAddressId { get; set; }
    public int? BillingAddressId { get; set; }
    public bool UseSameAddress { get; set; } = true;
}