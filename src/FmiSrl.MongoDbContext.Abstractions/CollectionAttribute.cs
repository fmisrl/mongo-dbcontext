using System.Diagnostics;

namespace FmiSrl.MongoDbContext.Abstractions;

[AttributeUsage(AttributeTargets.Property)]
[Conditional("false")]
public class CollectionAttribute(string Name) : Attribute;