using FmiSrl.MongoDbContext.Abstractions;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace FmiSrl.MongoDbContext.SourceGenerator.Tests;

public class TestObject : IAggregateRoot
{
    [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    public string? Id { get; set; } = null!;
    
    public string Name { get; set; } = null!;
}