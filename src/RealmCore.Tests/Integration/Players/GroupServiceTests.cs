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

        var group = await _groupsService.Create(name);

        using var _ = new AssertionScope();
        group!.Name.Should().Be(name);
        _groupsService.GetGroupByName(name).Should().NotBeNull();
        _groupsService.GetGroupById(group.Id).Should().NotBeNull();
    }
    
    [Fact]
    private async Task OnlyOneGroupOfGivenNameCanBeCreated()
    {
        var name = Guid.NewGuid().ToString();

        var result1 = await _groupsService.Create(name);
        var result2 = await _groupsService.Create(name);

        using var _ = new AssertionScope();
        result1.Should().NotBeNull();
        result2.Should().BeNull();
    }

    [Fact]
    private async Task MemberCanBeAddedToGivenGroupOnce()
    {
        var name = Guid.NewGuid().ToString();

        var group = await _groupsService.Create(name);

        var monitorPlayer = _groups.Monitor();
        var monitorService = _groupsService.Monitor();

        var added1 = await _groupsService.AddMember(_player, group!.Id);
        var added2 = await _groupsService.AddMember(_player, group!.Id);

        using var _ = new AssertionScope();
        added1.Should().BeTrue();
        added2.Should().BeFalse();
    }

    [Fact]
    private async Task MemberShouldBeAddedAndRemovePlayerFromGroup()
    {
        var name = Guid.NewGuid().ToString();

        var group = await _groupsService.Create(name);

        var monitorPlayer = _groups.Monitor();
        var monitorService = _groupsService.Monitor();
        var added = await _groupsService.AddMember(_player, group!.Id);
        var wasIngroup = _player.Groups.IsMember(group.Id);
        var removed = await _groupsService.RemoveMember(_player, group.Id);
        var isInGroup = _player.Groups.IsMember(group.Id);

        using var _ = new AssertionScope();
        added.Should().BeTrue();
        removed.Should().BeTrue();
        wasIngroup.Should().BeTrue();
        isInGroup.Should().BeFalse();
        monitorPlayer.GetOccurredEvents().Should().BeEquivalentTo(["VersionIncreased", "Added", "VersionIncreased", "Removed"]);
        monitorService.GetOccurredEvents().Should().BeEquivalentTo(["MemberAdded", "MemberRemoved"]);
    }
    
    [Fact]
    private async Task MemberDataShouldContainBasicInformationsAboutGroup()
    {
        var name = Guid.NewGuid().ToString();

        var group = await _groupsService.Create(name);

        await _groupsService.AddMember(_player, group!.Id);
        var members = await _groupsService.GetGroupMembersByUserId(_player.UserId);

        var groupMember1 = _player.Groups.Where(x => x.Group!.Name == name).FirstOrDefault();
        var groupMember2 = members.Where(x => x.Group!.Name == name).FirstOrDefault();

        using var _ = new AssertionScope();
        groupMember1.Should().NotBeNull();
        groupMember2.Should().NotBeNull();
    }
    
    [Fact]
    private async Task ChangingRoleShouldUpdatePermissions()
    {
        var name = Guid.NewGuid().ToString();

        var group = await _groupsService.Create(name);

        await _groupsService.AddMember(_player, group!.Id);

        var groupMember = _player.Groups.GetById(group.Id);

        {
            using var _ = new AssertionScope();
            groupMember!.Permissions.Should().BeEmpty();
            groupMember!.RoleId.Should().BeNull();
        }

        var groupRole = await _groupsService.CreateRole(group.Id, "foobar", [1, 2, 3]);
        var otherGroupRole = await _groupsService.CreateRole(group.Id, "other", [4, 5, 6]);
        await _groupsService.SetMemberRole(group.Id, _player.UserId, groupRole.Id);

        groupMember = _player.Groups.GetById(group.Id);

        {
            using var _ = new AssertionScope();
            groupMember!.Permissions.Should().BeEquivalentTo([1,2,3]);
            groupMember!.RoleId.Should().Be(groupRole.Id);
        }

        await _groupsService.SetRolePermissions(groupRole.Id, [100]);
        await _groupsService.SetRolePermissions(otherGroupRole.Id, [200]);

        groupMember = _player.Groups.GetById(group.Id);
        var permissionsFromDatabase = await _groupsService.GetGroupMemberPermissions(group.Id, _player.UserId);
        {
            using var _ = new AssertionScope();
            permissionsFromDatabase.Should().BeEquivalentTo([100]);
            groupMember!.Permissions.Should().BeEquivalentTo([100]);
        }
    }

    [Fact]
    private async Task GroupSettingsShouldBeChangeable()
    {
        var name = Guid.NewGuid().ToString();

        var group = await _groupsService.Create(name);

        await _groupsService.SetGroupSetting(group!.Id, 1, "foo");
        var settingValue = await _groupsService.GetGroupSetting(group.Id, 1);
        var settings = await _groupsService.GetGroupSettings(group.Id);

        using var _ = new AssertionScope();
        settingValue.Should().Be("foo");
        settings.Should().BeEquivalentTo(new Dictionary<int, string>
        {
            [1] = "foo"
        });
    }
    
    [Fact]
    private async Task SendingJoinRequestsShouldWork()
    {
        var name = Guid.NewGuid().ToString();

        var group = await _groupsService.Create(name);

        var sent = await _groupsService.CreateJoinRequest(group!.Id, _player);
        var requestsSentToGroups = await _groupsService.GetJoinRequestsByUserId(_player);
        var removed = await _groupsService.RemoveJoinRequest(group.Id, _player);

        using var _ = new AssertionScope();
        sent.Should().BeTrue();
        requestsSentToGroups.Select(x => x.GroupId).Should().Contain(group.Id);
        removed.Should().BeTrue();
    }
    
    [Fact]
    private async Task YouCanSendOnlyOneJoinRequest()
    {
        var name = Guid.NewGuid().ToString();

        var group = await _groupsService.Create(name);

        var sent1 = await _groupsService.CreateJoinRequest(group!.Id, _player);
        var sent2 = await _groupsService.CreateJoinRequest(group.Id, _player);

        using var _ = new AssertionScope();
        sent1.Should().BeTrue();
        sent2.Should().BeFalse();
    }
    
    [Fact]
    private async Task RemovingRoleShouldRemovePlayersFromRolesAndRevokePermissions()
    {
        var name = Guid.NewGuid().ToString();

        var group = await _groupsService.Create(name);

        var groupRole = await _groupsService.CreateRole(group!.Id, "foobar", [1, 2, 3]);
        await _groupsService.AddMember(_player, group.Id);
        await _groupsService.SetMemberRole(group.Id, _player.UserId, groupRole.Id);
        var removed1 = await _groupsService.RemoveRole(groupRole.Id);
        var removed2 = await _groupsService.RemoveRole(groupRole.Id);
        var monitorPlayer = _groups.Monitor();
        var isMember = _groups.TryGetMember(group.Id, out var member);

        using var _ = new AssertionScope();
        removed1.Should().BeTrue();
        removed2.Should().BeFalse();
        isMember.Should().BeTrue();
        member.RoleId.Should().BeNull();
        member.Permissions.Should().BeEmpty();
    }

    public void Dispose()
    {
        _player.Groups.Clear();
    }
}
