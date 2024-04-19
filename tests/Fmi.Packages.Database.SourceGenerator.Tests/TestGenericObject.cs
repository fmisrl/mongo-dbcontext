using Fmi.Packages.Database.Abstractions;

namespace Fmi.Packages.Database.SourceGenerator.Tests;

public class TestGenericObject<T> : IAggregateRoot
{
    public string? Id { get; set; }
}