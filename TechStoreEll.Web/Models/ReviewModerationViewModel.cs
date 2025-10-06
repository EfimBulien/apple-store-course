using TechStoreEll.Api.Entities;

namespace TechStoreEll.Web.Models;

public class ReviewModerationViewModel
{
    public List<Review> Reviews { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalUnmoderated { get; set; }
}