using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FmiSrl.MongoDbContext.SourceGenerator.Extensions;

internal static class PropertyDeclarationSyntaxExtensions
{
    public static string ToFullName(this PropertyDeclarationSyntax propertyDeclarationSyntax,
        SemanticModel semanticModel) =>
        propertyDeclarationSyntax.Type.ToFullName(semanticModel);

    private static string ToFullName(this TypeSyntax typeSyntax, SemanticModel semanticModel)
    {
        var fullName = "";

        switch (typeSyntax)
        {
            case PredefinedTypeSyntax predefinedTypeSyntax:
                return predefinedTypeSyntax.Keyword.Text;

            case GenericNameSyntax genericNameSyntax:
            {
                var typeName = semanticModel.GetTypeInfo(genericNameSyntax);

                fullName += typeName.Type?.ContainingNamespace + "." + typeName.Type?.Name;
                fullName += "<";

                fullName += string.Join(",",
                genericNameSyntax.TypeArgumentList.Arguments.Select(x => x.ToFullName(semanticModel)));

                fullName += ">";

                return fullName;
            }

            case IdentifierNameSyntax identifierNameSyntax:
            {
                var typeName = semanticModel.GetTypeInfo(identifierNameSyntax);

                fullName += typeName.Type?.ContainingNamespace + "." + typeName.Type?.Name;
                return fullName;
            }

            default:
                throw new NotSupportedException($"Unsupported type {typeSyntax.GetType()}");
        }
    }
}