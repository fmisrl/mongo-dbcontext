using System.Linq.Expressions;

namespace Fmi.Packages.ReflectionHelpers;

public static class ExpressionExtensions
{
    public static string GetPropertyName<TSource, TResult>(this Expression<Func<TSource, TResult>> exp)
    {
        if (exp.Body is not MemberExpression member)
            throw new ArgumentException("Expression is not a member access", nameof(exp));
        
        return member.Member.Name;
    }
}