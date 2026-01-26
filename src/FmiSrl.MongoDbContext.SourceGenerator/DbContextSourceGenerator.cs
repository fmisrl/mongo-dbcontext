using System.Text;
using FmiSrl.MongoDbContext.SourceGenerator.Analysis;
using FmiSrl.MongoDbContext.SourceGenerator.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FmiSrl.MongoDbContext.SourceGenerator;

[Generator]
public class DbContextSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Select(static (classDeclarationSyntax, _) => classDeclarationSyntax);

        var analyses = classDeclarations.Combine(context.CompilationProvider)
            .Select(static (pair, _) =>
            {
                var classDeclarationSyntax = pair.Left;
                var compilation = pair.Right;
                var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

                var hasModel = DbContextAnalyzer.TryCreateModel(
                    classDeclarationSyntax,
                    semanticModel,
                    compilation,
                    out var model,
                    out var diagnostics);

                return new DbContextAnalysis(hasModel ? model : null, diagnostics);
            });

        context.RegisterSourceOutput(analyses, static (ctx, analysis) =>
        {
            foreach (var diagnostic in analysis.Diagnostics)
            {
                ctx.ReportDiagnostic(diagnostic);
            }

            if (analysis.Model is null)
            {
                return;
            }

            var source = DbContextEmitter.Emit(analysis.Model);
            ctx.AddSource($"{analysis.Model.ClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }
}
