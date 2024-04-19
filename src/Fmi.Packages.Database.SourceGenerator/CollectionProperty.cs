namespace Fmi.Packages.Database.SourceGenerator;

internal class CollectionProperty
{
    public string Name { get; set; } = null!;
    
    public string CollectionName { get; set; } = null!;
    
    public string PropertyType { get; set; } = null!;
    
    public string MongoPropertyType { get; set; } = null!;
}