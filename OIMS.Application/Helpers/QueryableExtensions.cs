namespace OIMS.Application.Helpers
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(
        this IQueryable<T> query,
        int page = 1,
        int pageSize = 10)
        {
            if (page < 1)
            {
                throw new ArgumentException(
                    "Page must be greater than or equal to 1.",
                    nameof(page));
            }

            if (pageSize < 1)
            {
                throw new ArgumentException(
                    "Page size must be greater than 0.",
                    nameof(pageSize));
            }

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
        }
    }
}