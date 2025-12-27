using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EFProjectionCache;
using SampleApp.Data;
using SampleApp.Dtos;
using SampleApp.Entities;

Console.WriteLine("=== EF Projection Cache Sample ===\n");

// Setup services
var services = new ServiceCollection();

// Add projection cache with FusionCache
services.AddProjectionCache();

// Add EF Core with SQLite (using the interceptor from DI)
services.AddDbContext<SampleDbContext>((sp, options) =>
{
    options.UseSqlite("Data Source=sample.db");
    var interceptor = sp.GetRequiredService<ProjectionCacheInvalidationInterceptor>();
    options.AddInterceptors(interceptor);
});

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Get services
using var scope = serviceProvider.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
var cacheProvider = scope.ServiceProvider.GetRequiredService<IProjectionCacheProvider>();

// Ensure database is created
await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.EnsureCreatedAsync();

// Seed data
var company = new Company
{
    Name = "Acme Corporation",
    Address = "123 Main St, Springfield"
};

dbContext.Companies.Add(company);
await dbContext.SaveChangesAsync();

var user = new User
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@acme.com",
    CompanyId = company.Id
};

dbContext.Users.Add(user);
await dbContext.SaveChangesAsync();

// Clear change tracker to simulate fresh queries
dbContext.ChangeTracker.Clear();

Console.WriteLine("Data seeded successfully.\n");

// Helper function to determine if a type is an entity
bool IsEntityType(Type type) =>
    type == typeof(User) || type == typeof(Company);

// First call - will execute database query
Console.WriteLine("First call (cache miss - executes DB query):");
var users1 = await dbContext.Users
    .ProjectCachedAsync(
        u => new UserDto
        {
            Id = u.Id,
            FullName = u.FirstName + " " + u.LastName,
            Email = u.Email,
            CompanyName = u.Company.Name
        },
        cacheProvider,
        TimeSpan.FromMinutes(5),
        IsEntityType);

foreach (var u in users1)
{
    Console.WriteLine($"  - {u.FullName} ({u.Email}) at {u.CompanyName}");
}

// Second call - will use cache
Console.WriteLine("\nSecond call (cache hit - uses cached data):");
var users2 = await dbContext.Users
    .ProjectCachedAsync(
        u => new UserDto
        {
            Id = u.Id,
            FullName = u.FirstName + " " + u.LastName,
            Email = u.Email,
            CompanyName = u.Company.Name
        },
        cacheProvider,
        TimeSpan.FromMinutes(5),
        IsEntityType);

foreach (var u in users2)
{
    Console.WriteLine($"  - {u.FullName} ({u.Email}) at {u.CompanyName}");
}

// Update company - this will invalidate the cache
Console.WriteLine("\nUpdating company name from 'Acme Corporation' to 'Acme Corp (Updated)'...");
var companyToUpdate = await dbContext.Companies.FindAsync(company.Id);
companyToUpdate!.Name = "Acme Corp (Updated)";
await dbContext.SaveChangesAsync();

// Clear change tracker to demonstrate fresh data fetch
dbContext.ChangeTracker.Clear();

// Third call - cache should be invalidated, will execute DB query again
Console.WriteLine("\nThird call (cache invalidated by Company update - executes DB query):");
var users3 = await dbContext.Users
    .ProjectCachedAsync(
        u => new UserDto
        {
            Id = u.Id,
            FullName = u.FirstName + " " + u.LastName,
            Email = u.Email,
            CompanyName = u.Company.Name
        },
        cacheProvider,
        TimeSpan.FromMinutes(5),
        IsEntityType);

foreach (var u in users3)
{
    Console.WriteLine($"  - {u.FullName} ({u.Email}) at {u.CompanyName}");
}

Console.WriteLine("\n=== Sample completed successfully ===");
Console.WriteLine("\nThe projection cache correctly:");
Console.WriteLine("  1. Cached the first query result");
Console.WriteLine("  2. Returned cached result on second call");
Console.WriteLine("  3. Invalidated cache when Company entity was updated");
Console.WriteLine("  4. Re-executed the query to get fresh data with updated company name");
