namespace RealmCore.Tests.Integration.Fractions;

[Collection("IntegrationTests")]
public class FractionRepositoryTests : RealmRemoteDatabaseIntegrationTestingBase
{
    [Fact]
    public async Task FractionRepositoryShouldWork()
    {
        var server = await CreateServerAsync();
        var fractionRepository = server.GetRequiredService<IFractionRepository>();

        var fraction1 = await fractionRepository.CreateOrGet(1, "foo", "bar");
        var fraction2 = await fractionRepository.CreateOrGet(1, "foo", "bar");

        var expected = new FractionData
        {
            Id = 1,
            Name = "foo",
            Code = "bar",
            Members = [],
        };

        fraction1.Should().BeEquivalentTo(expected);
        fraction2.Should().BeEquivalentTo(expected);
    }
}
