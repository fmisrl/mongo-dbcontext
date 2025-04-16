using FmiSrl.MongoDbContext.Abstractions;

namespace FmiSrl.MongoDbContext.SourceGenerator.Tests;

public class TestGenericObject<T> : IAggregateRoot
{
    public string? Id { get; set; }
}