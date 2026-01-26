using Microsoft.CodeAnalysis;

namespace FmiSrl.MongoDbContext.SourceGenerator.Diagnostics;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor InvalidCollectionPropertyType = new(
        "DBCTX001",
        "Property not of type IMongoCollection",
        "Property not of type IMongoCollection",
        "SourceGenerator",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TooManyDbContextInterfaces = new(
        "DBCTX002",
        "Too many dbContext interfaces found",
        "Too many dbContext interfaces found",
        "SourceGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
