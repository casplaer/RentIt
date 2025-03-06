using RentIt.Users.Core.Interfaces.Specifications;
using System.Linq.Expressions;

namespace RentIt.Users.Application.Specifications
{
    public abstract class Specification<TEntity> : ISpecification<TEntity>
                where TEntity : class
    {
        protected Specification(Expression<Func<TEntity, bool>>? criteria) =>
            Criteria = criteria;

        public Expression<Func<TEntity, bool>>? Criteria { get; }

        private readonly List<Expression<Func<TEntity, object>>> _includeExpressions = new();

        public IReadOnlyList<Expression<Func<TEntity, object>>> IncludeExpressions => _includeExpressions;

        public Expression<Func<TEntity, object>>? OrderByExpression { get; private set; }

        public Expression<Func<TEntity, object>>? OrderByDescendingExpression { get; private set; }

        public int Page { get; private set; } = 1;

        public int PageSize { get; private set; } = 10;

        protected void AddInclude(Expression<Func<TEntity, object>> includeExpression) =>
            _includeExpressions.Add(includeExpression);

        protected void AddOrderBy(
            Expression<Func<TEntity, object>> orderByExpression) =>
            OrderByExpression = orderByExpression;

        protected void AddOrderByDescending(
            Expression<Func<TEntity, object>> orderByDescendingExpression) =>
            OrderByDescendingExpression = orderByDescendingExpression;

        protected void SetPagination(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
    }
}
