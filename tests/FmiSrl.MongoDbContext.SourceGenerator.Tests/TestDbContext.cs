namespace FmiSrl.MongoDbContext.SourceGenerator.Tests;

public partial class TestDbContext : ITestDbContext
{
    public string GetEmptyString => string.Empty;

    public string GetHelloString()
    {
        return "Say Hello!";
    }
}