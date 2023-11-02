namespace RealmCore.Tests.Tests.Services;

public class RealmCommandServiceTests
{
    [InlineData("foo", "FOO", true)]
    [InlineData("foo", "foo", true)]
    [InlineData("foo", "bar", false)]
    [Theory]
    public void YouCanNotCreateTwoSameCommands(string command1, string command2, bool shouldThrow)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var _sut = realmTestingServer.GetRequiredService<RealmCommandService>();

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
    public void YouCanNotCreateTwoSameCommandsMixedAsyncAndNotAsync(string command1, string command2, bool shouldThrow)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var _sut = realmTestingServer.GetRequiredService<RealmCommandService>();

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
    public void YouCanNotCreateTwoSameAsyncCommands(string command1, string command2, bool shouldThrow)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var _sut = realmTestingServer.GetRequiredService<RealmCommandService>();

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
        var realmTestingServer = new RealmTestingServer();
        var sut = realmTestingServer.GetRequiredService<RealmCommandService>();
        var player = realmTestingServer.CreatePlayer();
        await realmTestingServer.SignInPlayer(player);

        bool wasExecuted = false;
        void act(Element elements, CommandArguments args)
        {
            wasExecuted = true;
        }

        if (useAsyncHandler)
        {
            sut.AddAsyncCommandHandler("foo", (e, args, token) => { act(e, args); return Task.CompletedTask; });
        }
        else
        {
            sut.AddCommandHandler("foo", (e, args) => { act(e, args); });
        }

        player.TriggerCommand("foo", ["bar", "baz"]);
        wasExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task RateLimitShouldWork()
    {
        var realmTestingServer = new RealmTestingServer();
        var sut = realmTestingServer.GetRequiredService<RealmCommandService>();
        var player = realmTestingServer.CreatePlayer();
        await realmTestingServer.SignInPlayer(player);

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
        var realmTestingServer = new RealmTestingServer();
        var sut = realmTestingServer.GetRequiredService<RealmCommandService>();
        var player1 = realmTestingServer.CreatePlayer(name: "CrosRoad95");
        var player2 = realmTestingServer.CreatePlayer(name: "Index00");
        await realmTestingServer.SignInPlayer(player1);

        bool commandExecutedSuccessfully = false;
        sut.AddCommandHandler("foo", (e, args) => {
            args.ReadWord().Should().Be("bar");
            args.ReadInt().Should().Be(2);
            args.ReadPlayer().Should().Be(player2);
            args.End();
            commandExecutedSuccessfully = true;
        });
        player1.TriggerCommand("foo", ["bar", "2", "00"]);

        commandExecutedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task CommandArgumentsShouldWork2()
    {
        var realmTestingServer = new RealmTestingServer();
        var sut = realmTestingServer.GetRequiredService<RealmCommandService>();
        var player = realmTestingServer.CreatePlayer(name: "CrosRoad95");
        await realmTestingServer.SignInPlayer(player);

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
