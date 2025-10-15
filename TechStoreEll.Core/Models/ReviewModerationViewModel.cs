using TechStoreEll.Core.Entities;

namespace TechStoreEll.Core.Models;

public class ReviewModerationViewModel
{
    public List<Review> Reviews { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalUnmoderated { get; set; }
}