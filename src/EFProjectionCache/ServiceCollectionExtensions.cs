using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace EFProjectionCache;

/// <summary>
/// Extension methods for registering projection cache services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds projection cache services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureFusionCache">Optional action to configure FusionCache.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddProjectionCache(
        this IServiceCollection services,
        Action<FusionCacheOptions>? configureFusionCache = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Add FusionCache
        services.AddFusionCache()
            .WithDefaultEntryOptions(options =>
            {
                options.Duration = TimeSpan.FromMinutes(5);
            });

        if (configureFusionCache != null)
        {
            services.Configure(configureFusionCache);
        }

        // Register our cache provider
        services.AddSingleton<FusionProjectionCacheProvider>();
        services.AddSingleton<IProjectionCacheProvider>(sp =>
            sp.GetRequiredService<FusionProjectionCacheProvider>());
        services.AddSingleton<IProjectionCacheInvalidator>(sp =>
            sp.GetRequiredService<FusionProjectionCacheProvider>());

        // Register the interceptor
        services.AddSingleton<ProjectionCacheInvalidationInterceptor>();

        return services;
    }
}
