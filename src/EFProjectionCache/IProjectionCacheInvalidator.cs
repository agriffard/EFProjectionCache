namespace EFProjectionCache;

/// <summary>
/// Interface for invalidating cached projections by entity type.
/// </summary>
public interface IProjectionCacheInvalidator
{
    /// <summary>
    /// Invalidates all cached projections that depend on the specified entity type.
    /// </summary>
    /// <param name="entityType">The entity type that was changed.</param>
    void InvalidateByEntity(Type entityType);
}
