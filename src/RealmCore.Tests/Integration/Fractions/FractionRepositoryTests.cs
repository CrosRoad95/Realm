namespace RealmCore.Tests.Integration.Fractions;

public class FractionRepositoryTests : RealmIntegrationTestingBase
{
    protected override string DatabaseName => "FractionRepositoryTests";

    [Fact]
    public async Task FractionRepositoryShouldWork()
    {
        var server = await CreateServerAsync();
        var fractionRepository = server.GetRequiredService<IFractionRepository>();

        var fraction1 = await fractionRepository.TryCreateFraction(1, "foo", "bar");
        var fraction2 = await fractionRepository.TryCreateFraction(1, "foo", "bar");

        fraction1.Should().BeEquivalentTo(new FractionData
        {
            Id = 1,
            Name = "foo",
            Code = "bar",
            Members = [],
        });

        fraction2.Should().BeNull();
    }
}
