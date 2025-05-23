﻿using FmiSrl.MongoDbContext.Abstractions;
using MongoDB.Driver;

namespace FmiSrl.MongoDbContext.SourceGenerator.Tests;

public interface ITestDbContext : IDbContext
{
    [Abstractions.Collection("TestObjects")]
    IMongoCollection<TestObject> TestObjects { get; }
    
    IMongoCollection<TestGenericObject<string>> TestGenericObjects { get; }
    
    string GetEmptyString { get; }

    string GetHelloString();
}