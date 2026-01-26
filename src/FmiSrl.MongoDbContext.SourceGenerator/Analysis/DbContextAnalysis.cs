using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using FmiSrl.MongoDbContext.SourceGenerator.Models;

namespace FmiSrl.MongoDbContext.SourceGenerator.Analysis;

internal sealed class DbContextAnalysis
{
    public DbContextAnalysis(DbContextModel? model, ImmutableArray<Diagnostic> diagnostics)
    {
        Model = model;
        Diagnostics = diagnostics;
    }

    public DbContextModel? Model { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
}
