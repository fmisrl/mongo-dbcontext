using MongoDB.Bson;
using MongoDB.Driver;

namespace Fmi.Packages.Database.SourceGenerator.Tests.Integration;

public class DbContextTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public DbContextTests(DatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public void TestDbContext_ShouldImplementInterface_NoCondition()
    {
        var dbContext = new TestDbContext();
        Assert.IsAssignableFrom<ITestDbContext>(dbContext);
    }

    [Fact]
    public async Task TestDbContext_ShouldPing_NoCondition()
    {
        await _fixture.DbContext.Database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
    }

    [Fact]
    public void TestDbContext_TestTypes_NoCondition()
    {
        Assert.IsAssignableFrom<IMongoCollection<TestObject>>(_fixture.DbContext.TestObjects);
        Assert.IsAssignableFrom<IMongoCollection<TestGenericObject<string>>>(_fixture.DbContext.TestGenericObjects);
    }

    [Fact]
    public void GetCollectionFromType_TestTypes_NoCondition()
    {
        Assert.IsAssignableFrom<IMongoCollection<TestObject>>(_fixture.DbContext.GetCollectionFromType<TestObject>());
        Assert.IsAssignableFrom<IMongoCollection<TestGenericObject<string>>>(_fixture.DbContext
            .GetCollectionFromType<TestGenericObject<string>>());
    }
}