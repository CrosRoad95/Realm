namespace RealmCore.Tests.Repositories;

public class FractionRepositoryTests
{
    //[Fact]
    public async Task FractionRepositoryShouldWork()
    {
        var testingServer = new RealmTestingServer();
        var dateTimeProvider = testingServer.GetRequiredService<IDateTimeProvider>();
        var fractionRepository = testingServer.GetRequiredService<IFractionRepository>();

        var fraction1 = await fractionRepository.TryCreateFraction(1, "foo", "bar");
        var fraction2 = await fractionRepository.TryCreateFraction(1, "foo", "bar");

        fraction1.Should().Be(new FractionData
        {
            Id = 1,
            Name = "foo",
            Code = "bar",
            Members = [],
        });

        fraction2.Should().BeNull();
    }
}
