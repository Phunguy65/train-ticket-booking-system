namespace backend.Business.Models;

/// <summary>
/// Wraps paginated data with metadata for client-side pagination controls.
/// Provides information about total records, current page, and navigation state.
/// </summary>
public class PagedResult<T>
{
	public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
	public int TotalCount { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
	public bool HasPreviousPage => PageNumber > 1;
	public bool HasNextPage => PageNumber < TotalPages;
}