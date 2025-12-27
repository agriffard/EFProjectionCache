# EFProjectionCache

A strongly-typed projection cache for EF Core with automatic and transitive invalidation using FusionCache.

## Features

- **Strongly-typed cache** for EF Core projections based on LINQ expressions
- **Expression-based cache keys** - no fragile string keys
- **Automatic cache invalidation** via EF Core interceptor
- **Transitive dependency detection** - when related entities change, cached projections are invalidated
- **FusionCache integration** for efficient caching with tag-based invalidation

## Getting Started

### Installation

Add the EFProjectionCache package to your project:

```bash
dotnet add package EFProjectionCache
```

### Configuration

```csharp
// Setup services
var services = new ServiceCollection();

// Add projection cache with FusionCache
services.AddProjectionCache();

// Add EF Core with the cache invalidation interceptor
services.AddDbContext<YourDbContext>((sp, options) =>
{
    options.UseSqlServer("your-connection-string");
    var interceptor = sp.GetRequiredService<ProjectionCacheInvalidationInterceptor>();
    options.AddInterceptors(interceptor);
});
```

### Usage

```csharp
// Define your entity type predicate
bool IsEntityType(Type type) =>
    type == typeof(User) || type == typeof(Company);

// Use the ProjectCachedAsync extension method
var users = await dbContext.Users
    .ProjectCachedAsync(
        u => new UserDto
        {
            Id = u.Id,
            FullName = u.FirstName + " " + u.LastName,
            Email = u.Email,
            CompanyName = u.Company.Name  // Navigation property
        },
        cacheProvider,
        TimeSpan.FromMinutes(5),
        IsEntityType);
```

### How It Works

1. **Cache Key Generation**: The projection expression is analyzed to create a stable hash-based cache key
2. **Dependency Detection**: The `EntityDependencyVisitor` extracts all entity types referenced in the projection, including navigation properties
3. **Tagging**: The cached result is tagged with all dependent entity types
4. **Automatic Invalidation**: When `SaveChanges` is called, the interceptor detects modified entities and invalidates all cache entries tagged with those entity types

### Transitive Invalidation

The key feature is transitive invalidation. Given this projection:

```csharp
u => new UserDto { CompanyName = u.Company.Name }
```

The cache correctly identifies that:
- Root entity: `User`
- Navigation dependency: `Company`

If either `User` or `Company` entities are modified, the cached `UserDto` result is automatically invalidated.

## Sample Application

See the `samples/SampleApp` directory for a complete working example.

## Architecture

### Core Components

- **ProjectionCacheKey**: Immutable record representing a unique cache key
- **IProjectionCacheProvider**: Abstraction for cache operations
- **IProjectionCacheInvalidator**: Abstraction for cache invalidation
- **EntityDependencyVisitor**: Expression visitor that extracts entity dependencies
- **FusionProjectionCacheProvider**: FusionCache-based implementation
- **ProjectionCacheInvalidationInterceptor**: EF Core interceptor for automatic invalidation
- **QueryableExtensions**: IQueryable extension methods for cached projections

## License

MIT