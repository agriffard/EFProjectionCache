namespace EFProjectionCache;

/// <summary>
/// Abstraction for the projection cache provider.
/// </summary>
public interface IProjectionCacheProvider
{
    /// <summary>
    /// Gets a cached value or sets it using the factory if not present.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The projection cache key.</param>
    /// <param name="factory">The factory to create the value if not cached.</param>
    /// <param name="duration">The cache duration.</param>
    /// <param name="entityDependencies">The entity types that this projection depends on.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T> GetOrSetAsync<T>(
        ProjectionCacheKey key,
        Func<Task<T>> factory,
        TimeSpan duration,
        IReadOnlyCollection<Type> entityDependencies
    );
}
