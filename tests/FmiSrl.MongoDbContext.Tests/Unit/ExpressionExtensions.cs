using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FmiSrl.MongoDbContext.ReflectionHelpers;

namespace FmiSrl.MongoDbContext.Tests.Unit;

public sealed class ExpressionExtensions
{
    [Fact]
    public void GetPropertyName_Success_NoCondition()
    {
        // Arrange
        Expression<Func<TestClass, string>> expression = x => x.Property;

        // Act
        var memberName = expression.GetPropertyName();

        // Assert
        Assert.Equal(nameof(TestClass.Property), memberName);
    }
    
    [Fact]
    public void GetPropertyName_ThrowsArgumentException_WhenExpressionBodyIsNotMemberExpression()
    {
        // Arrange
        Expression<Func<TestClass, string>> expression = x => x.GetEmptyString();

        // Assert
        Assert.Throws<ArgumentException>(() => expression.GetPropertyName());
    }
    
    [ExcludeFromCodeCoverage]
    private class TestClass
    {
        public string Property { get; set; } = null!;
        
        public string GetEmptyString() => string.Empty;
    }
}