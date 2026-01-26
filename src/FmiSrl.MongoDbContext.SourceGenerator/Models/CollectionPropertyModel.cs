using Microsoft.CodeAnalysis;

namespace FmiSrl.MongoDbContext.SourceGenerator.Models;

internal sealed class CollectionPropertyModel
{
    public CollectionPropertyModel(
        string propertyName,
        ITypeSymbol documentType,
        ITypeSymbol collectionType,
        string collectionNameLiteral)
    {
        PropertyName = propertyName;
        DocumentType = documentType;
        CollectionType = collectionType;
        CollectionNameLiteral = collectionNameLiteral;
    }

    public string PropertyName { get; }
    
    public ITypeSymbol DocumentType { get; }
    
    public ITypeSymbol CollectionType { get; }
    
    public string CollectionNameLiteral { get; }
}
