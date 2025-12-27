PRD.md – Projection Cache for EF Core (with Transitive Invalidation)
1. Project Name

Projection Cache

Strongly-typed cache for EF Core projections based on LINQ expressions, with automatic and transitive invalidation.

2. Problem Statement (Pain)

DTO projections (Select) are recomputed unnecessarily

Identical SQL is executed multiple times

Cache keys based on strings are fragile

Cache invalidation is often forgotten or incorrect

Navigation-based projections become stale when related entities change

3. Goal

Provide a safe, strongly-typed projection cache for EF Core that:

Works on IQueryable

Uses expression trees as cache identity

Integrates with FusionCache

Automatically invalidates cached projections

Correctly handles navigation dependencies (transitive invalidation)

4. Target Audience

ASP.NET Core APIs

Back-office applications

EF Core–heavy systems

Clean Architecture / DDD projects

5. Core Concept

A projection is uniquely identified by:

Root entity type

Projection expression

DTO type

Optional query parameters

A projection also depends on all entities traversed by the expression.

6. Public API (Target)
var users = await db.Users
    .ProjectCached(
        u => new UserDto
        {
            Id = u.Id,
            CompanyName = u.Company.Name
        },
        duration: TimeSpan.FromMinutes(5)
    )
    .ToListAsync();

7. MVP Scope
Included

ProjectCached IQueryable extension

Expression-based cache key

Result caching (DTO result)

FusionCache provider

EF Core SaveChanges interceptor

Transitive invalidation via expression analysis

Excluded

Distributed cache coordination

Partial projection caching

SQL cost–based invalidation

8. Architecture Overview
8.1 Projection Cache Key
public sealed record ProjectionCacheKey(
    Type RootEntityType,
    Type DtoType,
    string ExpressionHash,
    string? ParametersHash
);

ExpressionHash is a stable hash of the normalized expression tree

ParametersHash is optional (tenant, filters, etc.)

9. Cache Provider Abstraction
public interface IProjectionCacheProvider
{
    Task<T> GetOrSetAsync<T>(
        ProjectionCacheKey key,
        Func<Task<T>> factory,
        TimeSpan duration,
        IReadOnlyCollection<Type> entityDependencies
    );
}

10. Transitive Dependency Detection (Key Feature)
Problem
u => u.Company.Name


Root entity: User

Navigation dependency: Company

If Company.Name changes, cached UserDto must be invalidated.

Solution

Analyze the projection expression tree to extract all entity types involved, including navigations.

Expression Visitor (Concept)
public sealed class EntityDependencyVisitor : ExpressionVisitor
{
    public HashSet<Type> EntityTypes { get; } = new();

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null &&
            typeof(IEntity).IsAssignableFrom(node.Expression.Type))
        {
            EntityTypes.Add(node.Expression.Type);
        }

        return base.VisitMember(node);
    }
}

Result
User
Company

11. Cache Tagging Strategy

Each cached projection is tagged with all dependent entity types.

Example tags:

User
Company


This guarantees correct invalidation when any dependent entity changes.

12. EF Core Invalidation Interceptor
Invalidator Interface
public interface IProjectionCacheInvalidator
{
    void InvalidateByEntity(Type entityType);
}

SaveChanges Interceptor
public sealed class ProjectionCacheInvalidationInterceptor : SaveChangesInterceptor
{
    private readonly IProjectionCacheInvalidator _invalidator;

    public ProjectionCacheInvalidationInterceptor(
        IProjectionCacheInvalidator invalidator)
    {
        _invalidator = invalidator;
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        Invalidate(eventData.Context);
        return result;
    }

    private void Invalidate(DbContext? context)
    {
        if (context == null) return;

        var entityTypes = context.ChangeTracker
            .Entries()
            .Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified ||
                e.State == EntityState.Deleted)
            .Select(e => e.Entity.GetType())
            .Distinct();

        foreach (var type in entityTypes)
            _invalidator.InvalidateByEntity(type);
    }
}

13. FusionCache Integration
public sealed class FusionProjectionCacheProvider
    : IProjectionCacheProvider, IProjectionCacheInvalidator
{
    private readonly IFusionCache _cache;

    public FusionProjectionCacheProvider(IFusionCache cache)
    {
        _cache = cache;
    }

    public Task<T> GetOrSetAsync<T>(
        ProjectionCacheKey key,
        Func<Task<T>> factory,
        TimeSpan duration,
        IReadOnlyCollection<Type> entityDependencies)
    {
        var cacheKey =
            $"{key.RootEntityType.FullName}:{key.DtoType.FullName}:{key.ExpressionHash}";

        return _cache.GetOrSetAsync(
            cacheKey,
            _ => factory(),
            options =>
            {
                options.Duration = duration;
                options.Tags = entityDependencies
                    .Select(t => t.FullName!)
                    .ToArray();
            });
    }

    public void InvalidateByEntity(Type entityType)
    {
        _cache.RemoveByTag(entityType.FullName!);
    }
}

14. Correctness Guarantee
Change	Cached Projection
User updated	UserDto invalidated
Company name updated	UserDto invalidated
Unrelated entity updated	No impact

Cache correctness is guaranteed by design.

15. Value Proposition

Correct by design

Strongly typed

No fragile string keys

Automatic consistency

High real-world adoption potential
