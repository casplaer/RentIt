using System.Linq.Expressions;

namespace RentIt.Bookings.Core.Interfaces.Specitfications
{
    public interface ISpecification<TEntity>
           where TEntity : class
    {
        Expression<Func<TEntity, bool>>? Criteria { get; }
        IReadOnlyList<Expression<Func<TEntity, object>>> IncludeExpressions { get; }
        Expression<Func<TEntity, object>>? OrderByExpression { get; }
        Expression<Func<TEntity, object>>? OrderByDescendingExpression { get; }
        int Page { get; }
        int PageSize { get; }
    }
}