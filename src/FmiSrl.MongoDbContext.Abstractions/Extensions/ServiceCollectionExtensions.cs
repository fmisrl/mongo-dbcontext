using Microsoft.Extensions.DependencyInjection;

namespace FmiSrl.MongoDbContext.Abstractions.Extensions;

/// <summary>
/// Database related extensions methods.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a transient db context to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="databaseName">The name of the database.</param>
    /// <typeparam name="TContext">The mongo database context.</typeparam>
    /// <typeparam name="TImpl">The mongo database context implementation.</typeparam>
    public static void AddMongoContext<TContext, TImpl>(this IServiceCollection serviceCollection,
        string connectionString, string databaseName)
        where TContext : class, IDbContext
        where TImpl : BsonDbContext<TImpl>, TContext, new()
    {
        serviceCollection.AddTransient<TContext>(_ =>
            ConnectionHelper.CreateContextAndConnectToDatabase<TContext, TImpl>(connectionString, databaseName));
    }

    
}