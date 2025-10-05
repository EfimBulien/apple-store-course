using TechStoreEll.Api.Entities;

namespace TechStoreEll.Api.Models;

public partial class Payment : IEntity
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Provider { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? PaidAt { get; set; }

    public string Status { get; set; } = null!;

    public string? TransactionRef { get; set; }

    public virtual Order Order { get; set; } = null!;
}
