namespace RealmCore.Tests.Unit.Players;

public class RealmCommandServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmCommandService _sut;
    private readonly RealmTestingPlayer _player;

    public RealmCommandServiceTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _sut = _fixture.Hosting.GetRequiredService<RealmCommandService>();
        _player = _fixture.Player;
    }

    [InlineData("foo", "FOO", true)]
    [InlineData("foo", "foo", true)]
    [InlineData("foo", "bar", false)]
    [Theory]
    public void YouCanNotCreateTwoSameCommands(string command1, string command2, bool shouldThrow)
    {
        var commandBase = Guid.NewGuid().ToString();
        command1 = $"{commandBase}{command1}";
        command2 = $"{commandBase}{command2}";
        var act = () =>
        {
            _sut.Add(command1, () => { });
            _sut.Add(command2, () => { });
        };

        if (shouldThrow)
        {
            act.Should().Throw<Exception>().WithMessage($"Command with name '{command2}' already exists");
        }
        else
        {
            act.Should().NotThrow();
        }
    }

    [Fact]
    public void RateLimitShouldWork()
    {
        var commandName = Guid.NewGuid().ToString();
        int executionCount = 0;
        _sut.Add(commandName, () => {
            executionCount++;
        });
        for (int i = 0; i < 50; i++)
            _player.TriggerCommand(commandName, []);

        executionCount.Should().Be(10);
    }

    [InlineData(null)]
    [InlineData(1337)]
    [Theory]
    public void CommandParsingShouldWork1(int? value)
    {
        var command = Guid.NewGuid().ToString();
        int outNumber1 = 0;
        int outNumber2 = 0;
        _sut.Add(command, (int number, int defaultNumber = 321) => {
            outNumber1 = number;
            outNumber2 = defaultNumber;
        });

        if(value != null)
        {
            _player.TriggerCommand(command, ["123", value.Value.ToString()]);
        }
        else
        {
            _player.TriggerCommand(command, ["123"]);
        }

        outNumber1.Should().Be(123);
        outNumber2.Should().Be(value != null ? 1337 : 321);
    }
}
