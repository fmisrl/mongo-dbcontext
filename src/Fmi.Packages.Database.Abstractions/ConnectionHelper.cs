namespace Fmi.Packages.Database.Abstractions;

public static class ConnectionHelper
{
    /// <summary>
    /// Connects to a db.
    /// </summary>
    /// <param name="connectionString">The connection string of the database.</param>
    /// <param name="databaseName">The name of the database.</param>
    /// <typeparam name="TContext">The mongo database context.</typeparam>
    /// <typeparam name="TImpl">The mongo database context implementation.</typeparam>
    public static TContext CreateContextAndConnectToDatabase<TContext, TImpl>(string connectionString, string databaseName)
        where TContext : IDbContext
        where TImpl : BsonDbContext<TImpl>, TContext, new()
    {
        TContext context = new TImpl();

        context.OpenConnection(connectionString, databaseName);

        return context;
    }
}