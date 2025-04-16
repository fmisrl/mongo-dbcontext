using FmiSrl.MongoDbContext.Abstractions.Exceptions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FmiSrl.MongoDbContext.Abstractions;

/// <summary>
/// A base class for a mongo database context.
/// </summary>
public abstract class BsonDbContext<TContext> : IDbContext where TContext : BsonDbContext<TContext>
{
    protected List<string> RegisteredMaps { get; } = [];

    private IMongoDatabase? _database;

    private IMongoClient? _mongoClient;

    public IMongoClient Client => _mongoClient ?? throw new InvalidOperationException("Client not available!");

    public IMongoDatabase Database => _database ?? throw new InvalidOperationException("Database not available!");

    /// <summary>
    /// Creates a new instance of the context with the default mongo client.
    /// </summary>
    protected BsonDbContext()
    {
    }

    /// <summary>
    /// Creates a new instance of the context with the provided client.
    /// </summary>
    /// <param name="mongoClient">The custom mongo client.</param>
    protected BsonDbContext(IMongoClient mongoClient) => _mongoClient = mongoClient;

    /// <inheritdoc />
    public abstract IMongoCollection<T> GetCollectionFromType<T>();


    /// <inheritdoc />
    public void OpenConnection(string connectionString, string databaseName)
    {
        var mongoConnectionUrl = new MongoUrl(connectionString);
        var mongoClientSettings = MongoClientSettings.FromUrl(mongoConnectionUrl);

        _mongoClient ??= new MongoClient(mongoClientSettings);

        OnConfiguringSchema();
        InitializeDatabase();

        _database = _mongoClient.GetDatabase(databaseName ?? throw new DatabaseNameNotProvided());
    }
    
    protected abstract void InitializeDatabase();

    /// <summary>
    /// Configures a schema when default settings are not applicable.
    /// </summary>
    protected virtual void OnConfiguringSchema()
    {
    }

    protected abstract string GetCollectionName<T>();


    /// <summary>
    /// Registers a map.
    /// </summary>
    /// <param name="classMap">Other map settings.</param>
    /// <typeparam name="T">The type of the map.</typeparam>
    protected void RegisterMap<T>(Action<BsonClassMap>? classMap = null)
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(T)))
        {
            return;
        }

        BsonClassMap.RegisterClassMap<T>(cm => CreateClassMap(cm, classMap));
    }

    private void CreateClassMap<T>(BsonClassMap<T> bsonClassMap, Action<BsonClassMap>? classMap = null) =>
        CreateClassMap(typeof(T), bsonClassMap, classMap);

    private void CreateClassMap(Type type, BsonClassMap bsonClassMap, Action<BsonClassMap>? classMap = null)
    {
        RegisteredMaps.Add(type.Name);

        bsonClassMap.AutoMap();
        bsonClassMap.SetIgnoreExtraElements(true);

        classMap?.Invoke(bsonClassMap);
    }
}