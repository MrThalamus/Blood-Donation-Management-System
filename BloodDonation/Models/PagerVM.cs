namespace BloodDonation.Models
{
    /// <summary>
    /// Paging state for a list screen. Deliberately knows nothing about what is
    /// being paged, so the _Pagination partial can be reused by any list.
    /// </summary>
    public class PagerVM
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalItems { get; set; }

        public int TotalPages =>
            TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);

        public bool HasPrevious => PageNumber > 1;

        public bool HasNext => PageNumber < TotalPages;

        // 1-based range of the rows currently on screen, for the "Showing x-y of z" caption.
        public int FirstItemNumber =>
            TotalItems == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;

        public int LastItemNumber =>
            Math.Min(PageNumber * PageSize, TotalItems);
    }
}
