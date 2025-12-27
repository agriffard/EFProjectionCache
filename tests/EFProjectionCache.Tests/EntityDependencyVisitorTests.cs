using System.Linq.Expressions;
using EFProjectionCache;

namespace EFProjectionCache.Tests;

public class EntityDependencyVisitorTests
{
    private class TestCompany
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TestUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TestCompany Company { get; set; } = null!;
    }

    private class TestUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
    }

    private static bool IsEntityType(Type type) =>
        type == typeof(TestUser) || type == typeof(TestCompany);

    [Fact]
    public void Visit_SimpleProjection_ExtractsRootEntity()
    {
        // Arrange
        Expression<Func<TestUser, TestUserDto>> projection = u => new TestUserDto
        {
            Id = u.Id,
            Name = u.Name
        };
        var visitor = new EntityDependencyVisitor(IsEntityType);

        // Act
        visitor.Visit(projection);

        // Assert
        Assert.Contains(typeof(TestUser), visitor.EntityTypes);
    }

    [Fact]
    public void Visit_NavigationProjection_ExtractsAllEntities()
    {
        // Arrange
        Expression<Func<TestUser, TestUserDto>> projection = u => new TestUserDto
        {
            Id = u.Id,
            Name = u.Name,
            CompanyName = u.Company.Name
        };
        var visitor = new EntityDependencyVisitor(IsEntityType);

        // Act
        visitor.Visit(projection);

        // Assert
        Assert.Contains(typeof(TestUser), visitor.EntityTypes);
        Assert.Contains(typeof(TestCompany), visitor.EntityTypes);
    }

    [Fact]
    public void CreateDefault_UsesIdHeuristic()
    {
        // Arrange
        Expression<Func<TestUser, TestUserDto>> projection = u => new TestUserDto
        {
            Id = u.Id,
            Name = u.Name
        };
        var visitor = EntityDependencyVisitor.CreateDefault();

        // Act
        visitor.Visit(projection);

        // Assert
        Assert.Contains(typeof(TestUser), visitor.EntityTypes);
    }
}
