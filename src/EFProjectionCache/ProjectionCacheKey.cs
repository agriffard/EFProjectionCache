namespace EFProjectionCache;

/// <summary>
/// Represents a unique cache key for a projection.
/// </summary>
/// <param name="RootEntityType">The root entity type being projected from.</param>
/// <param name="DtoType">The target DTO type being projected to.</param>
/// <param name="ExpressionHash">A stable hash of the normalized expression tree.</param>
/// <param name="ParametersHash">Optional hash for additional parameters (tenant, filters, etc.).</param>
public sealed record ProjectionCacheKey(
    Type RootEntityType,
    Type DtoType,
    string ExpressionHash,
    string? ParametersHash = null
);
