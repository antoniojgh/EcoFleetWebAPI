namespace EcoFleet.Application.Utilities.Common
{
    public class PaginatedDTO<T>
    {
        public int TotalCount { get; set; }
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    }
}
