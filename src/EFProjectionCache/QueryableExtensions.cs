using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace EFProjectionCache;

/// <summary>
/// Extension methods for IQueryable to enable cached projections.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Projects the query to a DTO type with caching support.
    /// </summary>
    /// <typeparam name="TEntity">The source entity type.</typeparam>
    /// <typeparam name="TDto">The target DTO type.</typeparam>
    /// <param name="queryable">The source queryable.</param>
    /// <param name="projection">The projection expression.</param>
    /// <param name="cacheProvider">The cache provider to use.</param>
    /// <param name="duration">The cache duration.</param>
    /// <param name="isEntityType">Optional predicate to determine entity types. If null, a default heuristic is used.</param>
    /// <param name="parametersHash">Optional hash for additional parameters.</param>
    /// <returns>A queryable that will use the cache.</returns>
    public static Task<List<TDto>> ProjectCachedAsync<TEntity, TDto>(
        this IQueryable<TEntity> queryable,
        Expression<Func<TEntity, TDto>> projection,
        IProjectionCacheProvider cacheProvider,
        TimeSpan duration,
        Func<Type, bool>? isEntityType = null,
        string? parametersHash = null)
        where TEntity : class
        where TDto : class
    {
        ArgumentNullException.ThrowIfNull(queryable);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(cacheProvider);

        // Compute expression hash
        var expressionHash = ExpressionHasher.ComputeHash(projection);

        // Create cache key
        var cacheKey = new ProjectionCacheKey(
            typeof(TEntity),
            typeof(TDto),
            expressionHash,
            parametersHash);

        // Extract entity dependencies
        var visitor = isEntityType != null
            ? new EntityDependencyVisitor(isEntityType)
            : EntityDependencyVisitor.CreateDefault();
        visitor.Visit(projection);

        // Always include the root entity type
        visitor.EntityTypes.Add(typeof(TEntity));

        // Execute with caching
        return cacheProvider.GetOrSetAsync(
            cacheKey,
            () => queryable.Select(projection).ToListAsync(),
            duration,
            visitor.EntityTypes);
    }

    /// <summary>
    /// Projects a single entity to a DTO type with caching support.
    /// </summary>
    /// <typeparam name="TEntity">The source entity type.</typeparam>
    /// <typeparam name="TDto">The target DTO type.</typeparam>
    /// <param name="queryable">The source queryable.</param>
    /// <param name="projection">The projection expression.</param>
    /// <param name="cacheProvider">The cache provider to use.</param>
    /// <param name="duration">The cache duration.</param>
    /// <param name="isEntityType">Optional predicate to determine entity types. If null, a default heuristic is used.</param>
    /// <param name="parametersHash">Optional hash for additional parameters.</param>
    /// <returns>The projected DTO or null if not found.</returns>
    public static Task<TDto?> ProjectCachedSingleAsync<TEntity, TDto>(
        this IQueryable<TEntity> queryable,
        Expression<Func<TEntity, TDto>> projection,
        IProjectionCacheProvider cacheProvider,
        TimeSpan duration,
        Func<Type, bool>? isEntityType = null,
        string? parametersHash = null)
        where TEntity : class
        where TDto : class
    {
        ArgumentNullException.ThrowIfNull(queryable);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(cacheProvider);

        // Compute expression hash
        var expressionHash = ExpressionHasher.ComputeHash(projection);

        // Create cache key
        var cacheKey = new ProjectionCacheKey(
            typeof(TEntity),
            typeof(TDto),
            expressionHash,
            parametersHash);

        // Extract entity dependencies
        var visitor = isEntityType != null
            ? new EntityDependencyVisitor(isEntityType)
            : EntityDependencyVisitor.CreateDefault();
        visitor.Visit(projection);

        // Always include the root entity type
        visitor.EntityTypes.Add(typeof(TEntity));

        // Execute with caching
        return cacheProvider.GetOrSetAsync(
            cacheKey,
            () => queryable.Select(projection).FirstOrDefaultAsync(),
            duration,
            visitor.EntityTypes);
    }
}
