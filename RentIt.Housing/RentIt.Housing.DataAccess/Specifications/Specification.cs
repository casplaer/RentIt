﻿using System.Linq.Expressions;

namespace RentIt.Housing.DataAccess.Specifications
{
    public abstract class Specification<TEntity> 
                    where TEntity : class
    {
        protected Specification(Expression<Func<TEntity, bool>>? criteria) =>
            Criteria = criteria;

        public Expression<Func<TEntity, bool>>? Criteria { get; }

        public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; } = new();

        public Expression<Func<TEntity, object>>? OrderByExpression { get; private set; }

        public Expression<Func<TEntity, object>>? OrderByDescendingExpression { get; private set; }

        public int Page { get; private set; } = 1;

        public int PageSize { get; private set; } = 10;

        protected void AddInclude(Expression<Func<TEntity, object>> includeExpression) =>
            IncludeExpressions.Add(includeExpression);

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
