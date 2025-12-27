using ZiggyCreatures.Caching.Fusion;

namespace EFProjectionCache;

/// <summary>
/// FusionCache-based implementation of the projection cache provider.
/// </summary>
public sealed class FusionProjectionCacheProvider : IProjectionCacheProvider, IProjectionCacheInvalidator
{
    private readonly IFusionCache _cache;

    /// <summary>
    /// Creates a new instance of the FusionProjectionCacheProvider.
    /// </summary>
    /// <param name="cache">The FusionCache instance to use.</param>
    public FusionProjectionCacheProvider(IFusionCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc />
    /// <remarks>
    /// The factory must return a non-null value. If T is a nullable reference type and the factory
    /// could return null, callers should handle this appropriately.
    /// </remarks>
    public async Task<T> GetOrSetAsync<T>(
        ProjectionCacheKey key,
        Func<Task<T>> factory,
        TimeSpan duration,
        IReadOnlyCollection<Type> entityDependencies)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(entityDependencies);

        var cacheKey = BuildCacheKey(key);
        var tags = entityDependencies.Select(t => t.FullName!).ToArray();

        var result = await _cache.GetOrSetAsync(
            cacheKey,
            async _ => await factory(),
            duration,
            tags);

        // The factory is expected to return a non-null value.
        // FusionCache's GetOrSetAsync will call the factory on cache miss and return the result.
        return result!;
    }

    /// <inheritdoc />
    public void InvalidateByEntity(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        _cache.RemoveByTag(entityType.FullName!);
    }

    private static string BuildCacheKey(ProjectionCacheKey key)
    {
        var baseKey = $"{key.RootEntityType.FullName}:{key.DtoType.FullName}:{key.ExpressionHash}";
        return key.ParametersHash != null
            ? $"{baseKey}:{key.ParametersHash}"
            : baseKey;
    }
}
