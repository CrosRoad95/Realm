namespace RealmCore.Tests.Integration.Players;

public class GroupServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly PlayerGroupsFeature _groups;
    private readonly GroupsService _groupsService;

    public GroupServiceTests(RealmTestingServerHostingFixtureWithPlayer fixture)
    {
        _fixture = fixture;
        _player = _fixture.Player;
        _groups = _player.Groups;
        _groupsService = _fixture.Hosting.GetRequiredService<GroupsService>();
    }

    [Fact]
    private async Task GroupCanBeCreated()
    {
        var name = Guid.NewGuid().ToString();

        var result = await _groupsService.Create(name);

        using var _ = new AssertionScope();

        ((GroupsResults.Created)result.Value).group.Name.Should().Be(name);
        _groupsService.GetGroupByName(name).Should().NotBeNull();
    }
    
    [Fact]
    private async Task OnlyOneGroupOfGivenNameCanBeCreated()
    {
        var name = Guid.NewGuid().ToString();

        var result1 = await _groupsService.Create(name);
        var result2 = await _groupsService.Create(name);

        using var _ = new AssertionScope();
        result1.Value.Should().BeOfType<GroupsResults.Created>();
        result2.Value.Should().BeOfType<GroupsResults.NameInUse>();
    }

    [Fact]
    private async Task MemberShouldBeAddedAndRemovePlayerFromGroup()
    {
        var name = Guid.NewGuid().ToString();

        var result = await _groupsService.Create(name);
        var group = ((GroupsResults.Created)result.Value).group;

        var monitorPlayer = _groups.Monitor();
        var monitorService = _groupsService.Monitor();
        var added = await _groupsService.TryAddMember(_player, group.Id);
        var removed = await _groupsService.TryAddMember(_player, group.Id);

        using var _ = new AssertionScope();
        added.Should().BeTrue();
        _player.Groups.IsMember(group.Id).Should().BeTrue();
        monitorPlayer.GetOccurredEvents().Should().BeEquivalentTo(["VersionIncreased", "Added"]);
        monitorService.GetOccurredEvents().Should().BeEquivalentTo(["MemberAdded"]);
    }

    public void Dispose()
    {
        _player.Groups.Clear();
    }
}
