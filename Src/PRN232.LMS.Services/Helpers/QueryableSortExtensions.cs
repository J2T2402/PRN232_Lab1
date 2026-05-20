using System.Linq.Expressions;

namespace PRN232.LMS.Services.Helpers;

public static class QueryableSortExtensions
{
    public static (IQueryable<TEntity> Query, IReadOnlyList<string> InvalidFields) ApplySort<TEntity>(
        this IQueryable<TEntity> query,
        string? sort,
        IReadOnlyDictionary<string, LambdaExpression> sortMappings,
        string defaultSortField)
    {
        IOrderedQueryable<TEntity>? orderedQuery = null;
        var invalidFields = new List<string>();
        var sortTokens = QueryParser.SplitCommaSeparated(sort);

        if (sortTokens.Count == 0)
        {
            if (sortMappings.TryGetValue(defaultSortField, out var defaultSelector))
            {
                orderedQuery = (IOrderedQueryable<TEntity>)ApplyOrdering(query, defaultSelector, descending: false, useThenBy: false);
            }

            return (orderedQuery ?? query, invalidFields);
        }

        foreach (var sortToken in sortTokens)
        {
            var descending = sortToken.StartsWith("-", StringComparison.Ordinal);
            var fieldName = descending ? sortToken[1..] : sortToken;
            var normalizedFieldName = fieldName.ToLowerInvariant();

            if (!sortMappings.TryGetValue(normalizedFieldName, out var sortSelector))
            {
                invalidFields.Add(fieldName);
                continue;
            }

            orderedQuery = (IOrderedQueryable<TEntity>)ApplyOrdering(
                orderedQuery ?? query,
                sortSelector,
                descending,
                useThenBy: orderedQuery is not null);
        }

        if (orderedQuery is null && sortMappings.TryGetValue(defaultSortField, out var fallbackSelector))
        {
            orderedQuery = (IOrderedQueryable<TEntity>)ApplyOrdering(query, fallbackSelector, descending: false, useThenBy: false);
        }

        return (orderedQuery ?? query, invalidFields);
    }

    private static object ApplyOrdering<TEntity>(
        IQueryable<TEntity> source,
        LambdaExpression keySelector,
        bool descending,
        bool useThenBy)
    {
        var methodName = useThenBy
            ? (descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy))
            : (descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy));

        var parameterTypes = useThenBy
            ? new[] { typeof(IOrderedQueryable<TEntity>), typeof(Expression<>) }
            : new[] { typeof(IQueryable<TEntity>), typeof(Expression<>) };

        var method = typeof(Queryable)
            .GetMethods()
            .Single(methodInfo =>
                methodInfo.Name == methodName &&
                methodInfo.IsGenericMethodDefinition &&
                methodInfo.GetParameters().Length == 2 &&
                methodInfo.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == parameterTypes[0].GetGenericTypeDefinition());

        var genericMethod = method.MakeGenericMethod(typeof(TEntity), keySelector.ReturnType);

        return genericMethod.Invoke(null, [source, keySelector])!;
    }
}
