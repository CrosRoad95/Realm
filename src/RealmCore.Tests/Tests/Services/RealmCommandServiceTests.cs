using SlipeServer.Server.Concepts;
using SlipeServer.Server.Events;

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
            _sut.AddAsyncCommandHandler(command1, (e, args) => { return Task.CompletedTask; });
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
            _sut.AddAsyncCommandHandler(command1, (e, args) => { return Task.CompletedTask; });
            _sut.AddAsyncCommandHandler(command2, (e, args) => { return Task.CompletedTask; });
        };

        if (shouldThrow)
        {
            act.Should().Throw<Exception>().WithMessage($"Async command with name '{command2}' already exists");
        }
        else
        {
            act.Should().NotThrow();
        }
    }

    //[InlineData(true)]
    //[InlineData(false)]
    //[Theory]
    public async Task AddCommandHandlerShouldWork(bool useAsyncHandler)
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var _sut = realmTestingServer.GetRequiredService<RealmCommandService>();

        bool wasExecuted = false;
        void act(Element elements, CommandArguments args)
        {
            wasExecuted = true;
        }

        if (useAsyncHandler)
        {
            _sut.AddAsyncCommandHandler("foo", (e, args) => { act(e, args); return Task.CompletedTask; });
            await _sut.InternalHandleAsyncTriggered(new Command("foo"), new CommandTriggeredEventArgs(player, new string[] {"bar", "baz"}));
        }
        else
        {
            _sut.AddCommandHandler("foo", (e, args) => { act(e, args); });
            await _sut.InternalHandleTriggered(new Command("foo"), new CommandTriggeredEventArgs(player, new string[] { "bar", "baz" }));
        }

        wasExecuted.Should().BeTrue();
    }
}
