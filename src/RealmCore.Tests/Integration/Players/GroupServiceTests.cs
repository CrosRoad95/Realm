namespace RealmCore.Tests.Integration.Players;

public class GroupServiceTests
{
    [Fact]
    public async Task GroupShouldBePossibleToCreate()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var groupService = hosting.GetRequiredService<GroupsService>();

        var groupName = Guid.NewGuid().ToString();

        await groupService.CreateGroup(groupName, groupName[..8], 0);

        var group = await groupService.GetGroupByName(groupName);

        group.Value.name.Should().Be(groupName);
    }

    //[Fact]
    public async Task YouCanNotCreateTwoGroupsWithTheSameName()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var groupService = hosting.GetRequiredService<GroupsService>();

        var createGroup = async () => await groupService.CreateGroup("foo", "TG2", 0);

        await createGroup.Should().NotThrowAsync();
        (await createGroup.Should().ThrowAsync<GroupNameInUseException>())
            .WithMessage($"Group 'foo' is already in use");
    }

    [Fact]
    public async Task YouCanAddMemberToGroup()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var groupService = hosting.GetRequiredService<GroupsService>();

        var groupName = Guid.NewGuid().ToString();
        var group = await groupService.CreateGroup(groupName, groupName[..8], 0);

        await groupService.TryAddMember(player, group.id, 1, "Leader");

        var member = player.Groups.GetMemberOrDefault(group.id) ?? throw new InvalidOperationException();
        var group2 = await groupService.GetGroupByName(groupName);
        member.GroupId.Should().Be(group.id);
        member.RankName.Should().Be("Leader");
        member.Rank.Should().Be(1);
        group2.Value.members.Should().HaveCount(1);
        group2.Value.members[0].userId.Should().Be(player.UserId);
    }

    //[Fact]
    public async Task YouCanAddMemberToGroupAndThenRemoveIt()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();

        var groupService = hosting.GetRequiredService<GroupsService>();
        var group = await groupService.CreateGroup("Test group4", "TG4", 0);

        await groupService.TryAddMember(player, group.id, 100, "Leader");
        var removed = await groupService.RemoveMember(player, player.UserId);

        player.Groups.IsMember(group.id).Should().BeFalse();
        var group2 = await groupService.GetGroupByName("Test group4");
        removed.Should().BeTrue();
        group2.Value.members.Should().BeEmpty();
    }
}
