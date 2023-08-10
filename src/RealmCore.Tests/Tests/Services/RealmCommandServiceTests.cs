using RealmCore.Persistence.Data;
using RealmCore.Server.Policies;
using SlipeServer.Server;
using SlipeServer.Server.Concepts;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Events;
using SlipeServer.Server.Services;

namespace RealmCore.Tests.Tests.Services;

public class RealmCommandServiceTests
{
    // CommandService commandService, ILogger<RealmCommandService> logger, IECS ecs, IUsersService usersService, IPolicyDrivenCommandExecutor policyDrivenCommandExecutor, ChatBox chatBox
    private readonly Mock<ILogger<RealmCommandService>> _logger = new(MockBehavior.Strict);
    private readonly CommandService _commandService;
    private readonly Mock<IECS> _ecsMock = new(MockBehavior.Strict);
    private readonly Mock<IUsersService> _usersServiceMock = new(MockBehavior.Strict);
    private readonly ChatBox _chatBox;
    private readonly PolicyDrivenCommandExecutor _policyDrivenCommandExecutor = new();
    private readonly RealmCommandService _sut;
    private readonly RealmTestingServer _server;
    private readonly EntityHelper _entityHelper;

    public RealmCommandServiceTests()
    {
        _server = new();
        _entityHelper = new(_server);
        _chatBox = new ChatBox(_server, _server.GetRequiredService<RootElement>());
        _commandService = new CommandService(_server);
        _logger.SetupLogger();
        _sut = new RealmCommandService(_commandService, _logger.Object, _ecsMock.Object, _usersServiceMock.Object, _policyDrivenCommandExecutor, _chatBox);
    }

    //[InlineData("foo", "FOO", true)]
    //[InlineData("foo", "foo", true)]
    //[InlineData("foo", "bar", false)]
    //[Theory]
    public void YouCanNotCreateTwoSameCommands(string command1, string command2, bool shouldThrow)
    {
        _sut.ClearCommands();
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

    //[InlineData("foo", "FOO", true)]
    //[InlineData("foo", "foo", true)]
    //[InlineData("foo", "bar", false)]
    //[Theory]
    public void YouCanNotCreateTwoSameCommandsMixedAsyncAndNotAsync(string command1, string command2, bool shouldThrow)
    {
        _sut.ClearCommands();
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

    //[InlineData("foo", "FOO", true)]
    //[InlineData("foo", "foo", true)]
    //[InlineData("foo", "bar", false)]
    //[Theory]
    public void YouCanNotCreateTwoSameAsyncCommands(string command1, string command2, bool shouldThrow)
    {
        _sut.ClearCommands();
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
        var player = new Player();
        var playerEntity = _entityHelper.CreatePlayerEntity();
        await playerEntity.AddComponentAsync<UserComponent>();

        _ecsMock.Setup(x => x.TryGetEntityByPlayer(player, out playerEntity, false)).Returns(true);
        _sut.ClearCommands();
        bool wasExecuted = false;
        var act = (Entity entity, CommandArguments args) =>
        {
            wasExecuted = true;
        };

        if(useAsyncHandler)
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
