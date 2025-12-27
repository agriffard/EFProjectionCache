using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFProjectionCache;

/// <summary>
/// EF Core SaveChanges interceptor that automatically invalidates cached projections
/// when entities are modified.
/// </summary>
public sealed class ProjectionCacheInvalidationInterceptor : SaveChangesInterceptor
{
    private readonly IProjectionCacheInvalidator _invalidator;

    /// <summary>
    /// Creates a new instance of the interceptor.
    /// </summary>
    /// <param name="invalidator">The cache invalidator to use.</param>
    public ProjectionCacheInvalidationInterceptor(IProjectionCacheInvalidator invalidator)
    {
        _invalidator = invalidator ?? throw new ArgumentNullException(nameof(invalidator));
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        InvalidateChangedEntities(eventData.Context);
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        InvalidateChangedEntities(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void InvalidateChangedEntities(DbContext? context)
    {
        if (context == null) return;

        var entityTypes = context.ChangeTracker
            .Entries()
            .Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified ||
                e.State == EntityState.Deleted)
            .Select(e => e.Entity.GetType())
            .Distinct()
            .ToList();

        foreach (var type in entityTypes)
        {
            _invalidator.InvalidateByEntity(type);
        }
    }
}
