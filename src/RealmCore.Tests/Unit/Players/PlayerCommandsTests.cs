namespace RealmCore.Tests.Unit.Players;

public class PlayerCommandsTests
{
    [InlineData("foo", "FOO", true)]
    [InlineData("foo", "foo", true)]
    [InlineData("foo", "bar", false)]
    [Theory]
    public async Task YouCanNotCreateTwoSameCommands(string command1, string command2, bool shouldThrow)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var worldObject = hosting.CreateObject();

        var _sut = hosting.GetRequiredService<RealmCommandService>();

        var act = () =>
        {
            _sut.AddCommandHandler(command1, (e, args) => { });
            _sut.AddCommandHandler(command2, (e, args) => { });
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

    [InlineData("foo", "FOO", true)]
    [InlineData("foo", "foo", true)]
    [InlineData("foo", "bar", false)]
    [Theory]
    public async Task YouCanNotCreateTwoSameCommandsMixedAsyncAndNotAsync(string command1, string command2, bool shouldThrow)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var _sut = hosting.GetRequiredService<RealmCommandService>();

        var act = () =>
        {
            _sut.AddCommandHandler(command2, (e, args) => { });
            _sut.AddAsyncCommandHandler(command1, (e, args, token) => { return Task.CompletedTask; });
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

    [InlineData("foo", "FOO", true)]
    [InlineData("foo", "foo", true)]
    [InlineData("foo", "bar", false)]
    [Theory]
    public async Task YouCanNotCreateTwoSameAsyncCommands(string command1, string command2, bool shouldThrow)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var _sut = hosting.GetRequiredService<RealmCommandService>();

        var act = () =>
        {
            _sut.AddAsyncCommandHandler(command1, (e, args, token) => { return Task.CompletedTask; });
            _sut.AddAsyncCommandHandler(command2, (e, args, token) => { return Task.CompletedTask; });
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

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task AddCommandHandlerShouldWork(bool useAsyncHandler)
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var sut = hosting.GetRequiredService<RealmCommandService>();

        var waitTask = new TaskCompletionSource();
        bool wasExecuted = false;
        void act(Element elements, CommandArguments args)
        {
            wasExecuted = true;
            waitTask.SetResult();
        }

        if (useAsyncHandler)
        {
            sut.AddAsyncCommandHandler("foo", (e, args, token) => { act(e, args); return Task.CompletedTask; });
        }
        else
        {
            sut.AddCommandHandler("foo", (e, args) => { act(e, args); });
        }

        //await Task.Delay(5000);
        player.TriggerCommand("foo", ["bar", "baz"]);
        await waitTask.Task;
        wasExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task RateLimitShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var sut = hosting.GetRequiredService<RealmCommandService>();

        int executionCount = 0;
        sut.AddCommandHandler("foo", (e, args) => {
            executionCount++;
        });
        for (int i = 0; i < 50; i++)
            player.TriggerCommand("foo", ["bar", "baz"]);

        executionCount.Should().Be(10);
    }

    [Fact]
    public async Task CommandArgumentsShouldWork1()
    {
        using var hosting = new RealmTestingServerHosting();
        var player1 = await hosting.CreatePlayer();
        var player2 = await hosting.CreatePlayer();
        var sut = hosting.GetRequiredService<RealmCommandService>();

        bool commandExecutedSuccessfully = false;
        sut.AddCommandHandler("foo", (e, args) => {
            args.ReadWord().Should().Be("bar");
            args.ReadInt().Should().Be(2);
            args.ReadPlayer().Should().Be(player2);
            args.End();
            commandExecutedSuccessfully = true;
        });
        player1.TriggerCommand("foo", ["bar", "2", "TestPlayer2"]);

        commandExecutedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task CommandArgumentsShouldWork2()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var sut = hosting.GetRequiredService<RealmCommandService>();

        bool tooManyArguments = false;
        sut.AddCommandHandler("foo", (e, args) => {
            args.ReadWord().Should().Be("bar");
            args.ReadInt().Should().Be(2);
            try
            {
                args.End();
            }
            catch (CommandArgumentException)
            {
                tooManyArguments = true;
            }
        });
        player.TriggerCommand("foo", ["bar", "2", "00"]);

        tooManyArguments.Should().BeTrue();
    }
}
