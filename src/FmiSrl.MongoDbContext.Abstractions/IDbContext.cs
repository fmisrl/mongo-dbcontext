using MongoDB.Driver;

namespace FmiSrl.MongoDbContext.Abstractions;

/// <summary>
/// A base database context.
/// </summary>
public interface IDbContext
{
    /// <summary>
    /// Retrieves a collection from the type.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <returns>The collection.</returns>
    IMongoCollection<T> GetCollectionFromType<T>();
    
    IMongoClient Client { get; }
    
    IMongoDatabase Database { get; }

    /// <summary>
    /// Creates a new connection.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="databaseName">The name of the database to connect.</param>
    void OpenConnection(string connectionString, string databaseName);
}