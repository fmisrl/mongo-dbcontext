using Fmi.Packages.Database.Abstractions;
using MongoDB.Driver;

namespace Fmi.Packages.Database.SourceGenerator.Tests;

public interface ITestDbContext : IDbContext
{
    IMongoCollection<TestObject> TestObjects { get; }
    
    IMongoCollection<TestGenericObject<string>> TestGenericObjects { get; }
    
    string GetEmptyString { get; }

    string GetHelloString();
}