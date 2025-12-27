using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace EFProjectionCache;

/// <summary>
/// Utility class for computing stable hashes from expression trees.
/// </summary>
public static class ExpressionHasher
{
    /// <summary>
    /// Computes a stable hash for the given expression.
    /// </summary>
    /// <param name="expression">The expression to hash.</param>
    /// <returns>A stable hash string.</returns>
    public static string ComputeHash(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var normalizedString = NormalizeExpression(expression);
        var bytes = Encoding.UTF8.GetBytes(normalizedString);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    private static string NormalizeExpression(Expression expression)
    {
        var visitor = new NormalizingExpressionVisitor();
        visitor.Visit(expression);
        return visitor.ToString();
    }

    private sealed class NormalizingExpressionVisitor : ExpressionVisitor
    {
        private readonly StringBuilder _builder = new();
        private int _parameterCounter;
        private readonly Dictionary<ParameterExpression, string> _parameterNames = [];

        public override string ToString() => _builder.ToString();

        public override Expression? Visit(Expression? node)
        {
            if (node == null) return null;

            _builder.Append(node.NodeType);
            _builder.Append(':');
            _builder.Append(node.Type.FullName);
            _builder.Append('|');

            return base.Visit(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (!_parameterNames.TryGetValue(node, out var name))
            {
                name = $"p{_parameterCounter++}";
                _parameterNames[node] = name;
            }

            _builder.Append(name);
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _builder.Append(node.Member.DeclaringType?.FullName);
            _builder.Append('.');
            _builder.Append(node.Member.Name);
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _builder.Append(node.Value?.ToString() ?? "null");
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            _builder.Append("new:");
            _builder.Append(node.Type.FullName);

            if (node.Members != null)
            {
                foreach (var member in node.Members)
                {
                    _builder.Append(',');
                    _builder.Append(member.Name);
                }
            }

            return base.VisitNew(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            _builder.Append("init:");
            _builder.Append(node.Type.FullName);

            foreach (var binding in node.Bindings)
            {
                _builder.Append(',');
                _builder.Append(binding.Member.Name);
            }

            return base.VisitMemberInit(node);
        }
    }
}
