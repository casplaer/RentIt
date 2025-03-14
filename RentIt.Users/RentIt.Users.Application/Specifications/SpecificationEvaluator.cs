using RentIt.Users.Core.Interfaces.Specifications;
using Microsoft.EntityFrameworkCore;

namespace RentIt.Users.Application.Specifications
{
    public static class SpecificationEvaluator
    {
        public static IQueryable<TEntity> GetQuery<TEntity>(
            IQueryable<TEntity> inputQueryable,
            ISpecification<TEntity> specification)
            where TEntity : class
        {
            IQueryable<TEntity> queryable = inputQueryable;

            if (specification.Criteria is not null)
            {
                queryable = queryable.Where(specification.Criteria);
            }

            queryable = specification.IncludeExpressions.Aggregate(
                queryable,
                (current, includeExpression) => current.Include(includeExpression));

            if (specification.OrderByExpression is not null)
            {
                queryable = queryable.OrderBy(specification.OrderByExpression);
            }
            else if (specification.OrderByDescendingExpression is not null)
            {
                queryable = queryable.OrderByDescending(specification.OrderByDescendingExpression);
            }

            queryable = queryable
                .Skip((specification.Page - 1) * specification.PageSize)
                .Take(specification.PageSize);

            return queryable;
        }
    }
}
