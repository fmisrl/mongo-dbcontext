namespace FmiSrl.MongoDbContext.SourceGenerator.Tests.Integration;

public class DatabaseFixture : IAsyncLifetime
{
    public ITestDbContext DbContext { get; private set; } = null!;

    private string _databaseName = null!;

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            _databaseName = Guid.NewGuid().ToString();

            DbContext = new TestDbContext();
            DbContext.OpenConnection(Environment.GetEnvironmentVariable("ConnectionStrings__Default")!, _databaseName);
        });
    }

    public async Task DisposeAsync() => await DbContext.Client.DropDatabaseAsync(_databaseName);
}