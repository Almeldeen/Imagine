using Application.Common.Enums;
using Application.Common.Interfaces;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Infrastructure.Services
{
    public class QueryService : IQueryService
    {
        private static readonly ConcurrentDictionary<string, PropertyInfo?> _propertyCache = new();
        private static readonly ConcurrentDictionary<string, MethodInfo?> _methodCache = new();

        public IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        public IQueryable<T> ApplySorting<T>(IQueryable<T> query, string? sortBy, SortDirection sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return query;
            }

            var cacheKey = $"{typeof(T).FullName}.{sortBy}";
            var propertyInfo = _propertyCache.GetOrAdd(cacheKey, _ =>
                typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            );

            if (propertyInfo == null)
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyInfo);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = sortDirection == SortDirection.Desc
                ? "OrderByDescending"
                : "OrderBy";

            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), propertyInfo.PropertyType },
                query.Expression,
                Expression.Quote(lambda)
            );

            return query.Provider.CreateQuery<T>(resultExpression);
        }

        public IQueryable<T> ApplySearch<T>(IQueryable<T> query, string? searchTerm, params string[] searchProperties)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || searchProperties.Length == 0)
            {
                return query;
            }

            var searchTermLower = searchTerm.ToLower().Trim();
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combinedExpression = null;

            var toLowerMethod = GetCachedMethod("String.ToLower", () => 
                typeof(string).GetMethod("ToLower", Type.EmptyTypes));
            var containsMethod = GetCachedMethod("String.Contains", () => 
                typeof(string).GetMethod("Contains", new[] { typeof(string) }));

            if (toLowerMethod == null || containsMethod == null)
            {
                return query;
            }

            foreach (var propertyName in searchProperties)
            {
                var cacheKey = $"{typeof(T).FullName}.{propertyName}";
                var propertyInfo = _propertyCache.GetOrAdd(cacheKey, _ =>
                    typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                );

                if (propertyInfo == null || propertyInfo.PropertyType != typeof(string))
                {
                    continue;
                }

                var property = Expression.Property(parameter, propertyInfo);
                var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
                var toLowerCall = Expression.Call(property, toLowerMethod);
                var containsCall = Expression.Call(toLowerCall, containsMethod, Expression.Constant(searchTermLower));
                var condition = Expression.AndAlso(nullCheck, containsCall);

                combinedExpression = combinedExpression == null
                    ? condition
                    : Expression.OrElse(combinedExpression, condition);
            }

            if (combinedExpression == null)
            {
                return query;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            return query.Where(lambda);
        }

        private static MethodInfo? GetCachedMethod(string key, Func<MethodInfo?> methodFactory)
        {
            return _methodCache.GetOrAdd(key, _ => methodFactory());
        }
    }
}
