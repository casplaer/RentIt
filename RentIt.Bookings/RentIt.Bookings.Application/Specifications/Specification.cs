using RentIt.Bookings.Core.Interfaces.Specitfications;
using System.Linq.Expressions;

namespace RentIt.Bookings.Application.Specifications
{
    public abstract class Specification<TEntity> : ISpecification<TEntity>
                    where TEntity : class
    {
        private readonly List<Expression<Func<TEntity, object>>> _includeExpressions = new();

        protected Specification(Expression<Func<TEntity, bool>>? criteria) =>
            Criteria = criteria;

        public Expression<Func<TEntity, bool>>? Criteria { get; }

        public IReadOnlyList<Expression<Func<TEntity, object>>> IncludeExpressions => _includeExpressions;

        public int? Page { get; private set; } = 1;

        public int? PageSize { get; private set; } = 10;

        protected void AddInclude(Expression<Func<TEntity, object>> includeExpression) =>
            _includeExpressions.Add(includeExpression);

        protected void SetPagination(int? page, int? pageSize)
        {
            if(page > 0 && pageSize > 0)
            {
                Page = page;
                PageSize = pageSize;
            }
        }
    }
}