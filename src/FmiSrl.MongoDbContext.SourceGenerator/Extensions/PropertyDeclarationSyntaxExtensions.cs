using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FmiSrl.MongoDbContext.SourceGenerator.Extensions;

internal static class PropertyDeclarationSyntaxExtensions
{
    public static string ToFullName(this PropertyDeclarationSyntax propertyDeclarationSyntax,
        SemanticModel semanticModel) =>
        propertyDeclarationSyntax.Type.ToFullName(semanticModel);

    public static string ToFullName(this TypeSyntax typeSyntax, SemanticModel semanticModel)
    {
        var fullName = "";

        if (typeSyntax is PredefinedTypeSyntax predefinedTypeSyntax)
            return predefinedTypeSyntax.Keyword.Text;

        if (typeSyntax is GenericNameSyntax genericNameSyntax)
        {
            var typeName = semanticModel.GetTypeInfo(genericNameSyntax);

            fullName += typeName.Type?.ContainingNamespace + "." + typeName.Type?.Name;
            fullName += "<";

            fullName += string.Join(",",
                genericNameSyntax.TypeArgumentList.Arguments.Select(x => x.ToFullName(semanticModel)));

            fullName += ">";

            return fullName;
        }

        if (typeSyntax is IdentifierNameSyntax identifierNameSyntax)
        {
            var typeName = semanticModel.GetTypeInfo(identifierNameSyntax);

            fullName += typeName.Type?.ContainingNamespace + "." + typeName.Type?.Name;
            return fullName;
        }

        throw new NotSupportedException($"Unsupported type {typeSyntax.GetType()}");
    }
}