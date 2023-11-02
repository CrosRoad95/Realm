namespace RealmCore.Tests.Tests;

public class GroupServiceTests
{
    [Fact]
    public async Task GroupShouldBePossibleToCreate()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var groupService = realmTestingServer.GetRequiredService<IGroupService>();

        await groupService.CreateGroup("foo", "TG1", GroupKind.Regular);

        var group = await groupService.GetGroupByName("foo");

        group.Value.name.Should().Be("foo");
    }

    //[Fact]
    public async Task YouCanNotCreateTwoGroupsWithTheSameName()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var groupService = realmTestingServer.GetRequiredService<IGroupService>();

        var createGroup = async () => await groupService.CreateGroup("foo", "TG2", GroupKind.Regular);

        await createGroup.Should().NotThrowAsync();
        (await createGroup.Should().ThrowAsync<GroupNameInUseException>())
            .WithMessage($"Group 'foo' is already in use");
    }

    [Fact]
    public async Task YouCanAddMemberToGroup()
    {
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        await realmTestingServer.SignInPlayer(player);
        var groupService = realmTestingServer.GetRequiredService<IGroupService>();

        var group = await groupService.CreateGroup("Test group3", "TG3", GroupKind.Regular);

        await groupService.AddMember(player, group.id, 1, "Leader");

        var groupMemberComponent = player.GetRequiredComponent<GroupMemberComponent>();
        var group2 = await groupService.GetGroupByName("Test group3");
        groupMemberComponent.GroupId.Should().Be(group.id);
        groupMemberComponent.RankName.Should().Be("Leader");
        groupMemberComponent.Rank.Should().Be(1);
        group2.Value.members.Should().HaveCount(1);
        group2.Value.members[0].userId.Should().Be(player.UserId);
    }

    //[Fact]
    public async Task YouCanAddMemberToGroupAndThenRemoveIt()
    {
        var userId = 1;
        var realmTestingServer = new RealmTestingServer();
        var player = realmTestingServer.CreatePlayer();
        var groupService = realmTestingServer.GetRequiredService<IGroupService>();
        var group = await groupService.CreateGroup("Test group4", "TG4", GroupKind.Regular);

        await groupService.AddMember(player, group.id, 100, "Leader");
        var removed = await groupService.RemoveMember(player, userId);

        player.HasComponent<GroupMemberComponent>().Should().BeFalse();
        var group2 = await groupService.GetGroupByName("Test group4");
        removed.Should().BeTrue();
        group2.Value.members.Should().BeEmpty();
    }
}
