using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FmiSrl.MongoDbContext.Abstractions.Extensions;

/// <summary>
/// Db context extensions.
/// </summary>
public static class DbContextExtensions
{
    private const int KeyDefaultLifetime = 180;

    /// <summary>
    /// Configures the Db Context and the required xml repository.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="databaseName">The name of the database.</param>
    public static void ConfigureDbContext<TContext, TImpl>(this IServiceCollection serviceCollection,
        string connectionString, string databaseName)
        where TContext : class, IDbContext
        where TImpl : BsonDbContext<TImpl>, TContext, new()
    {
        serviceCollection.AddMongoContext<TContext, TImpl>(connectionString, databaseName);

        serviceCollection.AddDataProtection()
            .SetDefaultKeyLifetime(TimeSpan.FromDays(KeyDefaultLifetime))
            .Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(serviceProvider =>
                new ConfigureOptions<KeyManagementOptions>(options =>
                    options.XmlRepository = serviceProvider.GetRequiredService<IXmlRepository>()
                )
            );
    }
}