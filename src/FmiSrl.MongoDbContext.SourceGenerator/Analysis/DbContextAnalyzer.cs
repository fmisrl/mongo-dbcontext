using System.Collections.Immutable;
using FmiSrl.MongoDbContext.SourceGenerator.Consts;
using FmiSrl.MongoDbContext.SourceGenerator.Diagnostics;
using FmiSrl.MongoDbContext.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FmiSrl.MongoDbContext.SourceGenerator.Analysis;

internal static class DbContextAnalyzer
{
    public static bool TryCreateModel(
        ClassDeclarationSyntax classDeclarationSyntax,
        SemanticModel semanticModel,
        Compilation compilation,
        out DbContextModel model,
        out ImmutableArray<Diagnostic> diagnostics)
    {
        var diagnosticsBuilder = ImmutableArray.CreateBuilder<Diagnostic>();
        model = null!;

        if (ModelExtensions.GetDeclaredSymbol(semanticModel, classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
        {
            diagnostics = diagnosticsBuilder.ToImmutable();
            return false;
        }

        var dbContextInterfaceSymbol = compilation.GetTypeByMetadataName(
            $"{AbstractionProjectConsts.AbstractionsNamespace}.{AbstractionProjectConsts.DbContextInterfaceName}");

        if (dbContextInterfaceSymbol is null)
        {
            diagnostics = diagnosticsBuilder.ToImmutable();
            return false;
        }

        var dbContextInterfaces = classSymbol.AllInterfaces
            .Where(interfaceSymbol =>
                interfaceSymbol.AllInterfaces.Contains(dbContextInterfaceSymbol, SymbolEqualityComparer.Default))
            .ToImmutableArray();

        if (dbContextInterfaces.Length == 0)
        {
            diagnostics = diagnosticsBuilder.ToImmutable();
            return false;
        }

        if (dbContextInterfaces.Length > 1)
        {
            diagnosticsBuilder.Add(Diagnostic.Create(
                DiagnosticDescriptors.TooManyDbContextInterfaces,
                classDeclarationSyntax.Identifier.GetLocation()));
            diagnostics = diagnosticsBuilder.ToImmutable();
            return false;
        }

        var interfaceSymbol = dbContextInterfaces[0];
        var mongoCollectionSymbol = compilation.GetTypeByMetadataName("MongoDB.Driver.IMongoCollection`1");
        var collectionAttributeSymbol = compilation.GetTypeByMetadataName(
            $"{AbstractionProjectConsts.AbstractionsNamespace}.CollectionAttribute");

        if (mongoCollectionSymbol is null)
        {
            diagnostics = diagnosticsBuilder.ToImmutable();
            return false;
        }

        var collections = ImmutableArray.CreateBuilder<CollectionPropertyModel>();

        foreach (var propertySymbol in interfaceSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (propertySymbol.Type is not INamedTypeSymbol propertyType ||
                !propertyType.IsGenericType ||
                !SymbolEqualityComparer.Default.Equals(propertyType.ConstructedFrom, mongoCollectionSymbol))
            {
                var location = propertySymbol.Locations.FirstOrDefault();
                diagnosticsBuilder.Add(Diagnostic.Create(
                    DiagnosticDescriptors.InvalidCollectionPropertyType,
                    location));
                continue;
            }

            var documentType = propertyType.TypeArguments[0];
            var collectionNameLiteral = SymbolDisplay.FormatLiteral(propertySymbol.Name, quote: true);

            if (collectionAttributeSymbol is not null)
            {
                var collectionAttribute = propertySymbol.GetAttributes()
                    .FirstOrDefault(attribute =>
                        SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, collectionAttributeSymbol));

                if (collectionAttribute is not null &&
                    collectionAttribute.ConstructorArguments.Length > 0 &&
                    collectionAttribute.ConstructorArguments[0].Value is string collectionName)
                {
                    collectionNameLiteral = SymbolDisplay.FormatLiteral(collectionName, quote: true);
                }
            }

            collections.Add(new CollectionPropertyModel(
                propertySymbol.Name,
                documentType,
                propertyType,
                collectionNameLiteral));
        }

        var classModifiers = classDeclarationSyntax.Modifiers.ToFullString().Trim();

        model = new DbContextModel(
            classSymbol.ContainingNamespace.ToDisplayString(),
            classSymbol.Name,
            classModifiers,
            interfaceSymbol,
            collections.ToImmutable());

        diagnostics = diagnosticsBuilder.ToImmutable();
        return true;
    }
}
