namespace RentIt.Users.Core.Interfaces.Repositories
{
    public class PaginatedResult<T>
    {
        public ICollection<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
