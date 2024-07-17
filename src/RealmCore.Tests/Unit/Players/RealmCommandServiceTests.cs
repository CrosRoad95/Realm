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

    private bool CommandThrottling
    {
        set
        {
            _player.GetRequiredService<ICommandThrottlingPolicy>().Enabled = value;
        }
    }

    [InlineData("foo", "FOO", true)]
    [InlineData("foo", "foo", true)]
    [InlineData("foo", "bar", false)]
    [Theory]
    public void YouCanNotCreateTwoSameCommands(string command1, string command2, bool shouldThrow)
    {
        CommandThrottling = false;

        var commandBase = Guid.NewGuid();
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
        CommandThrottling = true;

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
        CommandThrottling = false;

        var waitHandle = new AutoResetEvent(false);
        var command = $"CommandParsingShouldWork1{Guid.NewGuid()}";
        int outNumber1 = -1;
        int outNumber2 = -1;

        _sut.Add(command, (int number, int defaultNumber = 321) => {
            outNumber1 = number;
            outNumber2 = defaultNumber;
            waitHandle.Set();
        });

        if(value != null)
        {
            _player.TriggerCommand(command, ["123", value.Value.ToString()]);
        }
        else
        {
            _player.TriggerCommand(command, ["123"]);
        }

        if (!waitHandle.WaitOne(1000))
            throw new TimeoutException();

        using var _ = new AssertionScope();

        outNumber1.Should().Be(123);
        outNumber2.Should().Be(value != null ? 1337 : 321);
    }
}
