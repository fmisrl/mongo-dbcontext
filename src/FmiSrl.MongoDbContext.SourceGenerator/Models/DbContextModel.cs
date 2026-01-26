using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace FmiSrl.MongoDbContext.SourceGenerator.Models;

internal sealed class DbContextModel
{
    public DbContextModel(
        string @namespace,
        string className,
        string classModifiers,
        INamedTypeSymbol interfaceSymbol,
        ImmutableArray<CollectionPropertyModel> collections)
    {
        Namespace = @namespace;
        ClassName = className;
        ClassModifiers = classModifiers;
        InterfaceSymbol = interfaceSymbol;
        Collections = collections;
    }

    public string Namespace { get; }
    public string ClassName { get; }
    public string ClassModifiers { get; }
    public INamedTypeSymbol InterfaceSymbol { get; }
    public ImmutableArray<CollectionPropertyModel> Collections { get; }
}
