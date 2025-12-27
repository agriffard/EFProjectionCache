using System.Linq.Expressions;
using EFProjectionCache;

namespace EFProjectionCache.Tests;

public class ExpressionHasherTests
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
    public void ComputeHash_SameExpression_ReturnsSameHash()
    {
        // Arrange
        Expression<Func<TestEntity, TestDto>> expression1 = e => new TestDto
        {
            Id = e.Id,
            Name = e.Name
        };

        Expression<Func<TestEntity, TestDto>> expression2 = e => new TestDto
        {
            Id = e.Id,
            Name = e.Name
        };

        // Act
        var hash1 = ExpressionHasher.ComputeHash(expression1);
        var hash2 = ExpressionHasher.ComputeHash(expression2);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_DifferentExpressions_ReturnsDifferentHashes()
    {
        // Arrange
        Expression<Func<TestEntity, TestDto>> expression1 = e => new TestDto
        {
            Id = e.Id,
            Name = e.Name
        };

        Expression<Func<TestEntity, TestDto>> expression2 = e => new TestDto
        {
            Id = e.Id
        };

        // Act
        var hash1 = ExpressionHasher.ComputeHash(expression1);
        var hash2 = ExpressionHasher.ComputeHash(expression2);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_NullExpression_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ExpressionHasher.ComputeHash(null!));
    }

    [Fact]
    public void ComputeHash_ReturnsNonEmptyString()
    {
        // Arrange
        Expression<Func<TestEntity, TestDto>> expression = e => new TestDto { Id = e.Id };

        // Act
        var hash = ExpressionHasher.ComputeHash(expression);

        // Assert
        Assert.False(string.IsNullOrEmpty(hash));
    }
}
