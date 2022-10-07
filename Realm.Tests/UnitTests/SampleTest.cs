namespace Realm.Tests.UnitTests;

[CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
public class SampleTest : IClassFixture<ServerFixture>
{
    private readonly ServerFixture _serverFixture;

    public SampleTest(ServerFixture serverFixture)
    {
        _serverFixture = serverFixture;
    }

    [Fact]
    public void Test1()
    {
        Assert.True(_serverFixture.TestServer != null);
    }
}