using System.Data.Entity;

namespace RentIt.Users.Application.Specifications
{
    public static class SpecificationEvaluator
    {
        public static IQueryable<TEntity> GetQuery<TEntity>(
            IQueryable<TEntity> inputQueryable,
            Specification<TEntity> specification)
            where TEntity : class
        {
            IQueryable<TEntity> queryable = inputQueryable;

            if(specification.Criteria is not null)
            {
                queryable = queryable.Where(specification.Criteria);
            }

            queryable = specification.IncludeExpressions.Aggregate(
                queryable,
                (current, includeExpresion) =>
                    current.Include(includeExpresion));

            if(specification.OrderByExpression is not null)
            {
                queryable = queryable.OrderBy(specification.OrderByExpression);
            }
            else if (specification.OrderByDescendingExpression is not null)
            {
                queryable = queryable.OrderByDescending(
                    specification.OrderByDescendingExpression);
            }

            return queryable;
        }
    }
}
