using Microsoft.EntityFrameworkCore;
using EFProjectionCache;
using ZiggyCreatures.Caching.Fusion;

namespace EFProjectionCache.Tests;

public class FusionProjectionCacheProviderTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task GetOrSetAsync_FirstCall_ExecutesFactory()
    {
        // Arrange
        var fusionCache = new FusionCache(new FusionCacheOptions());
        var provider = new FusionProjectionCacheProvider(fusionCache);
        var key = new ProjectionCacheKey(typeof(TestEntity), typeof(TestDto), "hash1");
        var factoryCallCount = 0;

        // Act
        var result = await provider.GetOrSetAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return new TestDto { Id = 1, Name = "Test" };
            },
            TimeSpan.FromMinutes(5),
            [typeof(TestEntity)]);

        // Assert
        Assert.Equal(1, factoryCallCount);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task GetOrSetAsync_SecondCall_UsesCachedValue()
    {
        // Arrange
        var fusionCache = new FusionCache(new FusionCacheOptions());
        var provider = new FusionProjectionCacheProvider(fusionCache);
        var key = new ProjectionCacheKey(typeof(TestEntity), typeof(TestDto), "hash1");
        var factoryCallCount = 0;

        // Act
        var result1 = await provider.GetOrSetAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return new TestDto { Id = 1, Name = "Test" };
            },
            TimeSpan.FromMinutes(5),
            [typeof(TestEntity)]);

        var result2 = await provider.GetOrSetAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return new TestDto { Id = 2, Name = "Different" };
            },
            TimeSpan.FromMinutes(5),
            [typeof(TestEntity)]);

        // Assert
        Assert.Equal(1, factoryCallCount);
        Assert.Equal(result1.Id, result2.Id);
        Assert.Equal(result1.Name, result2.Name);
    }

    [Fact]
    public async Task InvalidateByEntity_InvalidatesCache()
    {
        // Arrange
        var fusionCache = new FusionCache(new FusionCacheOptions());
        var provider = new FusionProjectionCacheProvider(fusionCache);
        var key = new ProjectionCacheKey(typeof(TestEntity), typeof(TestDto), "hash1");
        var factoryCallCount = 0;

        // First call
        await provider.GetOrSetAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return new TestDto { Id = 1, Name = "Test" };
            },
            TimeSpan.FromMinutes(5),
            [typeof(TestEntity)]);

        // Act - invalidate
        provider.InvalidateByEntity(typeof(TestEntity));

        // Second call after invalidation
        var result = await provider.GetOrSetAsync(
            key,
            async () =>
            {
                factoryCallCount++;
                return new TestDto { Id = 2, Name = "Updated" };
            },
            TimeSpan.FromMinutes(5),
            [typeof(TestEntity)]);

        // Assert
        Assert.Equal(2, factoryCallCount);
        Assert.Equal(2, result.Id);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public void Constructor_NullCache_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FusionProjectionCacheProvider(null!));
    }
}
