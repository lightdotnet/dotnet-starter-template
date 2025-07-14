namespace Monolith
{
    /// <summary>
    /// Lookup data entries with pagination
    /// </summary>
    public record Pagination : IPage
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}