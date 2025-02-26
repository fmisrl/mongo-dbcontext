﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Fmi.Packages.Database.SourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Fmi.Packages.Database.SourceGenerator;

[Generator]
public class DbContextSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var dbContextProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => GetDbContextDeclarationForSourceGen(ctx)
            )
            .Where(t => t.dbContextClassFound)
            .Select((t, _) => t.Item1);

        context.RegisterSourceOutput(context.CompilationProvider.Combine(dbContextProvider.Collect()),
            ((ctx, t) => GenerateCode(ctx, t.Left, t.Right)));
    }

    private void GenerateCode(SourceProductionContext ctx, Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classesDeclarationSyntaxes)
    {
        foreach (var classDeclarationSyntax in classesDeclarationSyntaxes)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
            var dbContextSymbolInfo = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(classDeclarationSyntax)!;

            var dbContextInterfaceFound = dbContextSymbolInfo.AllInterfaces.Where(interfaceSymbol =>
                    interfaceSymbol.AllInterfaces.Any(x =>
                        x.Name == "IDbContext"
                        && x.ContainingNamespace.ToDisplayString() == "Fmi.Packages.Database.Abstractions"
                    )
                )
                .ToArray();

            if (dbContextInterfaceFound.Length is 0) continue;

            if (dbContextInterfaceFound.Length > 1)
            {
                var tooManyDbContextFound1 = new DiagnosticDescriptor("DBCTX002",
                    "Too many dbContext interfaces found",
                    "Too many dbContext interfaces found",
                    "SourceGenerator",
                    DiagnosticSeverity.Error,
                    true);

                var tooManyDbContextFoundErrorInstancea =
                    Diagnostic.Create(tooManyDbContextFound1, classDeclarationSyntax.GetLocation());

                ctx.ReportDiagnostic(tooManyDbContextFoundErrorInstancea);
                continue;
            }

            var interfaceNamedSymbol = dbContextInterfaceFound.First();

            var code = "// <auto-generated/>\r\n";
            code += "// " + DateTime.Now + "\r\n";

            code += """
                    using MongoDB.Bson.Serialization;
                    using MongoDB.Driver;
                    """;

            code += "\r\n\r\nnamespace " + dbContextSymbolInfo?.ContainingNamespace + "\r\n{\r\n";
            code += "    " + classDeclarationSyntax.Modifiers + " class " + classDeclarationSyntax.Identifier.Text +
                    " : global::Fmi.Packages.Database.Abstractions.BsonDbContext<" +
                    classDeclarationSyntax.Identifier.Text + ">, global::" +
                    interfaceNamedSymbol?.ContainingNamespace + "." + interfaceNamedSymbol?.Name +
                    "\r\n    {\r\n";

            var interfaceDeclarationSyntax = interfaceNamedSymbol?.DeclaringSyntaxReferences
                                                 .Select(x => x.GetSyntax())
                                                 .OfType<InterfaceDeclarationSyntax>()
                                             ?? [];

            var propertiesInTheClass = interfaceDeclarationSyntax.SelectMany(x => x.Members)
                .OfType<PropertyDeclarationSyntax>();

            var collectionProperties = new List<CollectionProperty>();

            foreach (var propertyDeclarationSyntax in propertiesInTheClass)
            {
                var descendentNodes = propertyDeclarationSyntax.DescendantNodes().ToList();

                var propertyTypeSyntax = descendentNodes.OfType<GenericNameSyntax>()
                    .FirstOrDefault();

                if (propertyTypeSyntax is null || propertyTypeSyntax.Identifier.Text != "IMongoCollection")
                {
                    var propertyTypeSyntaxError = new DiagnosticDescriptor("DBCTX001",
                        "Property not of type IMongoCollection",
                        $"Property not of type IMongoCollection",
                        "SourceGenerator",
                        DiagnosticSeverity.Info,
                        true);

                    var propertyTypeSyntaxErrorInstance =
                        Diagnostic.Create(propertyTypeSyntaxError, propertyDeclarationSyntax.GetLocation());

                    ctx.ReportDiagnostic(propertyTypeSyntaxErrorInstance);
                    continue;
                }

                var modifier = "        " + (propertyDeclarationSyntax.Modifiers.Any()
                    ? propertyDeclarationSyntax.Modifiers.ToFullString()
                    : "public");

                var regex = new Regex("^.*IMongoCollection<(?<objectType>.*)>$");
                var propertySemanticModel = compilation.GetSemanticModel(propertyDeclarationSyntax.SyntaxTree);

                var mongoPropertyType = propertyDeclarationSyntax.ToFullName(propertySemanticModel);
                var match = regex.Match(mongoPropertyType);

                var collectionName = "\"" + propertyDeclarationSyntax.Identifier.Text + "\"";

                foreach (var attributeSyntax in propertyDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes))
                {
                    var attributeSemanticModel = compilation.GetSemanticModel(attributeSyntax.SyntaxTree);
                    var attributeSymbol = attributeSemanticModel.GetTypeInfo(attributeSyntax);

                    if (attributeSymbol.Type?.Name == "CollectionAttribute" &&
                        attributeSymbol.Type.ContainingNamespace.ToDisplayString() ==
                        "Fmi.Packages.Database.Abstractions")
                    {
                        var collectionNameArgument = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault();
                        
                        if (collectionNameArgument is not null)
                        {
                             collectionName = collectionNameArgument.Expression.ToFullString();
                        }
                    }
                }

                var collectionProperty = new CollectionProperty
                {
                    Name = propertyDeclarationSyntax.Identifier.Text,
                    CollectionName = collectionName,
                    MongoPropertyType = mongoPropertyType,
                    PropertyType = match.Groups["objectType"].Value
                };

                collectionProperties.Add(collectionProperty);

                code +=
                    $"{modifier} global::{collectionProperty.MongoPropertyType} {collectionProperty.Name} => " +
                    $"Database.GetCollection<global::{collectionProperty.PropertyType}>({collectionProperty.CollectionName});\r\n\r\n";
            }

            code += "        protected override string GetCollectionName<T>()\r\n";
            code += "        {\r\n";

            var padding = "            ";

            foreach (var collectionProperty in collectionProperties)
            {
                code += $"{padding}if(typeof(T) == typeof(global::{collectionProperty.PropertyType})) {{\r\n"
                        + $"{padding}    return {collectionProperty.CollectionName};\r\n{padding}}}\r\n";
            }

            code += $"{padding}throw new System.NotSupportedException($\"Unsupported type {{typeof(T)}}\");\r\n";
            code += "        }\r\n";

            code += "\r\n";

            code += "        public override IMongoCollection<T> GetCollectionFromType<T>()\r\n";
            code += "        {\r\n";

            foreach (var collectionProperty in collectionProperties)
            {
                code += $"{padding}if(typeof(T) == typeof(global::{collectionProperty.PropertyType})) {{\r\n"
                        + $"{padding}    return (IMongoCollection<T>)this.{collectionProperty.Name};\r\n{padding}}}\r\n";
            }

            code += $"{padding}throw new System.NotSupportedException($\"Unsupported type {{typeof(T)}}\");\r\n";
            code += "        }\r\n";

            code += "\r\n";

            code += "        protected override void InitializeDatabase()\r\n";
            code += "        {\r\n";

            foreach (var collectionProperty in collectionProperties)
            {
                code +=
                    $"{padding}if(!RegisteredMaps.Contains(nameof(global::{collectionProperty.PropertyType}))) {{\r\n";
                code += $"{padding}    RegisterMap<global::{collectionProperty.PropertyType}>();\r\n";
                code += $"{padding}}}\r\n";
            }

            code += "        }\r\n";


            code += "    }\r\n";
            code += "}\r\n";

            ctx.AddSource($"{classDeclarationSyntax.Identifier.Text}.g.cs",
                SourceText.From(code, Encoding.UTF8));
        }
    }

    private static (InterfaceDeclarationSyntax, bool dbContextInterfaceFound) GetInterfaceDeclarationForSourceGen(
        GeneratorSyntaxContext context)
    {
        var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;
        var symbolInfo = context.SemanticModel.GetDeclaredSymbol(context.Node);

        if (symbolInfo is not INamedTypeSymbol namedTypeSymbol)
        {
            return (interfaceDeclarationSyntax, false);
        }

        var dbContextInterfaceFound = namedTypeSymbol.AllInterfaces.Any(x =>
            x.Name == "IDbContext"
            && x.ContainingNamespace.ToDisplayString() == "Fmi.Packages.Database.Abstractions"
        );

        return (interfaceDeclarationSyntax, dbContextInterfaceFound);
    }

    private static (ClassDeclarationSyntax, bool dbContextClassFound) GetDbContextDeclarationForSourceGen(
        GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var symbolInfo = context.SemanticModel.GetDeclaredSymbol(context.Node);

        if (symbolInfo is not INamedTypeSymbol namedTypeSymbol)
        {
            return (classDeclarationSyntax, false);
        }

        var dbContextInterfaceFound = namedTypeSymbol.AllInterfaces.Any(interfaceSymbol =>
            {
                return interfaceSymbol.AllInterfaces.Any(x =>
                    x.Name == "IDbContext"
                    && x.ContainingNamespace.ToDisplayString() == "Fmi.Packages.Database.Abstractions"
                );
            }
        );

        return (classDeclarationSyntax, dbContextInterfaceFound);
    }
}