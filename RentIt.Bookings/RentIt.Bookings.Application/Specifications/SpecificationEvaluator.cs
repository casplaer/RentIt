using Microsoft.EntityFrameworkCore;
using RentIt.Bookings.Core.Interfaces.Specitfications;

namespace RentIt.Bookings.Application.Specifications
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

            if(specification.Page != null && specification.PageSize != null)
            {
                queryable = queryable
                    .Skip((int)((specification.Page - 1) * specification.PageSize))
                    .Take((int)specification.PageSize);
            }

            return queryable;
        }
    }
}