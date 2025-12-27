using System.Linq.Expressions;
using System.Reflection;

namespace EFProjectionCache;

/// <summary>
/// Expression visitor that extracts all entity types involved in a projection expression,
/// including navigations for transitive dependency detection.
/// </summary>
public sealed class EntityDependencyVisitor : ExpressionVisitor
{
    private readonly Func<Type, bool> _isEntityType;

    /// <summary>
    /// Gets the set of entity types detected in the expression.
    /// </summary>
    public HashSet<Type> EntityTypes { get; } = [];

    /// <summary>
    /// Creates a new instance of the EntityDependencyVisitor.
    /// </summary>
    /// <param name="isEntityType">A predicate to determine if a type is an entity type.</param>
    public EntityDependencyVisitor(Func<Type, bool> isEntityType)
    {
        _isEntityType = isEntityType ?? throw new ArgumentNullException(nameof(isEntityType));
    }

    /// <summary>
    /// Creates a visitor that uses a simple heuristic: types with an "Id" property are entities.
    /// </summary>
    public static EntityDependencyVisitor CreateDefault()
    {
        return new EntityDependencyVisitor(type =>
            type.IsClass &&
            !type.IsAbstract &&
            type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance) != null);
    }

    /// <inheritdoc />
    protected override Expression VisitMember(MemberExpression node)
    {
        // Check if the expression type is an entity
        if (node.Expression != null && _isEntityType(node.Expression.Type))
        {
            EntityTypes.Add(node.Expression.Type);
        }

        // Also check the member's declaring type
        if (node.Member.DeclaringType != null && _isEntityType(node.Member.DeclaringType))
        {
            EntityTypes.Add(node.Member.DeclaringType);
        }

        return base.VisitMember(node);
    }

    /// <inheritdoc />
    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (_isEntityType(node.Type))
        {
            EntityTypes.Add(node.Type);
        }

        return base.VisitParameter(node);
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Handle navigation collection methods like .Select()
        if (node.Arguments.Count > 0)
        {
            foreach (var arg in node.Arguments)
            {
                if (arg.Type.IsGenericType)
                {
                    var genericArgs = arg.Type.GetGenericArguments();
                    foreach (var genericArg in genericArgs)
                    {
                        if (_isEntityType(genericArg))
                        {
                            EntityTypes.Add(genericArg);
                        }
                    }
                }
            }
        }

        return base.VisitMethodCall(node);
    }
}
