using Microsoft.EntityFrameworkCore;

namespace RentIt.Users.Core.Specifications
{
    public static class SpecificationEvaluator
    {
        public static IQueryable<TEntity> GetQuery<TEntity>(
            IQueryable<TEntity> inputQueryable,
            Specification<TEntity> specification)
            where TEntity : class
        {
            IQueryable<TEntity> queryable = inputQueryable;

            if (specification.Criteria is not null)
            {
                queryable = queryable.Where(specification.Criteria);
            }

            queryable = specification.IncludeExpressions.Aggregate(
                queryable,
                (current, includeExpresion) =>
                    current.Include(includeExpresion));

            if (specification.OrderByExpression is not null)
            {
                queryable = queryable.OrderBy(specification.OrderByExpression);
            }
            else if (specification.OrderByDescendingExpression is not null)
            {
                queryable = queryable.OrderByDescending(
                    specification.OrderByDescendingExpression);
            }

            queryable = queryable.Skip((specification.Page - 1) * specification.PageSize)
                     .Take(specification.PageSize);

            return queryable;
        }
    }
}
