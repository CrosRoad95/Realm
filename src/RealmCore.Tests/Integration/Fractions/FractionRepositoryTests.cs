namespace RealmCore.Tests.Integration.Fractions;

public class FractionRepositoryTests
{
    [Fact]
    public async Task FractionRepositoryShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var fractionRepository = hosting.GetRequiredService<IFractionRepository>();

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
