using Fmi.Packages.Database.Abstractions;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Fmi.Packages.Database.SourceGenerator.Tests;

public class TestObject : IAggregateRoot
{
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    public string? Id { get; set; } = null!;
    
    public string Name { get; set; } = null!;
}