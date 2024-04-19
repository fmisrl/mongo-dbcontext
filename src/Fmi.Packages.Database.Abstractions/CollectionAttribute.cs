using System.Diagnostics;

namespace Fmi.Packages.Database.Abstractions;

[AttributeUsage(AttributeTargets.Property)]
[Conditional("false")]
public class CollectionAttribute(string Name) : Attribute;