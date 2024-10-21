using RealmCore.Server.Modules.Players.Groups;

namespace RealmCore.Tests.Integration.Players;

public class GroupsServiceTests : IClassFixture<RealmTestingServerHostingFixtureWithPlayer>, IDisposable
{
    private readonly RealmTestingServerHostingFixtureWithPlayer _fixture;
    private readonly RealmTestingPlayer _player;
    private readonly PlayerGroupsFeature _groups;
    private readonly GroupsService _groupsService;

    public GroupsServiceTests(RealmTestingServerHostingFixtureWithPlayer fixture)
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
        var groupName = await _groupsService.GetGroupName(group!.Id);

        using var _ = new AssertionScope();
        groupName.Should().Be(name);
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
        var otherGroupRoleName = await _groupsService.GetRoleName(otherGroupRole.Id);
        await _groupsService.SetMemberRole(group.Id, _player.UserId, groupRole.Id);

        groupMember = _player.Groups.GetById(group.Id);

        {
            using var _ = new AssertionScope();
            otherGroupRoleName.Should().Be("other");
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
    private async Task YouShouldBeAbleToSendOnlyOneJoinRequestToGroup()
    {
        var name = Guid.NewGuid().ToString();

        var group = await _groupsService.Create(name);

        var sent1 = await _groupsService.CreateJoinRequest(group!.Id, _player);
        var sent2 = await _groupsService.CreateJoinRequest(group!.Id, _player);

        using var _ = new AssertionScope();
        sent1.Should().BeTrue();
        sent2.Should().BeFalse();
    }
    
    [Fact]
    private async Task YouShouldBeAbleToRemoveAllJoinRequests()
    {
        var name = Guid.NewGuid().ToString();

        var group = await _groupsService.Create(name);

        var sent = await _groupsService.CreateJoinRequest(group!.Id, _player);
        await _groupsService.RemoveAllJoinRequestsByUserId(_player.UserId);
        var requestsSentToGroups = await _groupsService.GetJoinRequestsByUserId(_player);

        using var _ = new AssertionScope();
        sent.Should().BeTrue();
        requestsSentToGroups.Should().BeEmpty();
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
    
    [Fact]
    private async Task AddingAndRemovingUpgradesShouldWork()
    {
        var group1 = await _groupsService.Create(Guid.NewGuid().ToString());
        var group2 = await _groupsService.Create(Guid.NewGuid().ToString());
        await _groupsService.AddUpgrade(group2!.Id, 1);
        await _groupsService.AddUpgrade(group2!.Id, 2);

        await _groupsService.AddMember(_player, group1!.Id);
        var upgradesIds1 = _groups.Upgrades;
        
        await _groupsService.AddMember(_player, group2!.Id);
        var upgradesIds2 = _groups.Upgrades;

        await _groupsService.AddUpgrade(group1!.Id, 2);
        await _groupsService.AddUpgrade(group1!.Id, 3);
        var upgradesIds3 = _groups.Upgrades;

        await _groupsService.RemoveMember(_player, group2!.Id);
        var upgradesIds4 = _groups.Upgrades;

        using var _ = new AssertionScope();

        upgradesIds1.Should().BeEmpty();
        upgradesIds2.Should().BeEquivalentTo([1, 2]);
        upgradesIds3.Should().BeEquivalentTo([1, 2, 3]);
        upgradesIds4.Should().BeEquivalentTo([2, 3]);
        _groups.HasUpgrade(2).Should().BeTrue();
        _groups.HasUpgrade(1).Should().BeFalse();
    }
    
    [Fact]
    private async Task GroupMemberStatisticsShouldWork()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var group = await _groupsService.Create(Guid.NewGuid().ToString());
        await _groupsService.AddMember(_player, group!.Id);
        await _groupsService.IncreaseStatistic(group!.Id, _player.UserId, 1, today, 10);
        await _groupsService.IncreaseStatistic(group!.Id, _player.UserId, 1, today, 10);
        await _groupsService.IncreaseStatistic(group!.Id, _player.UserId, 2, today, 10);
        var statistics1 = await _groupsService.GetStatisticsByUserId(group!.Id, _player.UserId, today);
        var statistics2 = await _groupsService.GetStatistics(group!.Id, [1, 2], today);

        using var _ = new AssertionScope();
        statistics1.Should().BeEquivalentTo([
            new GroupMemberStatistic(today, 1, 20),
            new GroupMemberStatistic(today, 2, 10),
        ]);
        statistics2.Should().BeEquivalentTo([
            new GroupMemberStatistic(today, 1, 20),
            new GroupMemberStatistic(today, 2, 10),
        ]);
    }

    public void Dispose()
    {
        _player.Groups.Clear();
    }
}
