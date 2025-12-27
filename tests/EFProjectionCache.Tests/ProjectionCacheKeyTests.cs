using EFProjectionCache;

namespace EFProjectionCache.Tests;

public class ProjectionCacheKeyTests
{
    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var key1 = new ProjectionCacheKey(typeof(string), typeof(int), "hash1", "params1");
        var key2 = new ProjectionCacheKey(typeof(string), typeof(int), "hash1", "params1");

        // Act & Assert
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void Equals_DifferentHash_ReturnsFalse()
    {
        // Arrange
        var key1 = new ProjectionCacheKey(typeof(string), typeof(int), "hash1", "params1");
        var key2 = new ProjectionCacheKey(typeof(string), typeof(int), "hash2", "params1");

        // Act & Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void Equals_DifferentEntityType_ReturnsFalse()
    {
        // Arrange
        var key1 = new ProjectionCacheKey(typeof(string), typeof(int), "hash1", "params1");
        var key2 = new ProjectionCacheKey(typeof(object), typeof(int), "hash1", "params1");

        // Act & Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void ParametersHash_IsOptional()
    {
        // Arrange
        var key = new ProjectionCacheKey(typeof(string), typeof(int), "hash1");

        // Assert
        Assert.Null(key.ParametersHash);
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHash()
    {
        // Arrange
        var key1 = new ProjectionCacheKey(typeof(string), typeof(int), "hash1", "params1");
        var key2 = new ProjectionCacheKey(typeof(string), typeof(int), "hash1", "params1");

        // Act & Assert
        Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
    }
}
