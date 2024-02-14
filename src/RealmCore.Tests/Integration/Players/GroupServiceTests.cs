namespace RealmCore.Tests.Integration.Players;

public class GroupServiceTests : RealmIntegrationTestingBase
{
    protected override string DatabaseName => "PlayerNotificationsTests";

    [Fact]
    public async Task GroupShouldBePossibleToCreate()
    {
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();

        var groupService = server.GetRequiredService<IGroupService>();

        await groupService.CreateGroup("foo", "TG1", GroupKind.Regular);

        var group = await groupService.GetGroupByName("foo");

        group.Value.name.Should().Be("foo");
    }

    //[Fact]
    public async Task YouCanNotCreateTwoGroupsWithTheSameName()
    {
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();

        var groupService = server.GetRequiredService<IGroupService>();

        var createGroup = async () => await groupService.CreateGroup("foo", "TG2", GroupKind.Regular);

        await createGroup.Should().NotThrowAsync();
        (await createGroup.Should().ThrowAsync<GroupNameInUseException>())
            .WithMessage($"Group 'foo' is already in use");
    }

    [Fact]
    public async Task YouCanAddMemberToGroup()
    {
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();

        var groupService = server.GetRequiredService<IGroupService>();

        var group = await groupService.CreateGroup("Test group3", "TG3", GroupKind.Regular);

        await groupService.TryAddMember(player, group.id, 1, "Leader");

        var member = player.Groups.GetMemberOrDefault(group.id) ?? throw new InvalidOperationException();
        var group2 = await groupService.GetGroupByName("Test group3");
        member.GroupId.Should().Be(group.id);
        member.RankName.Should().Be("Leader");
        member.Rank.Should().Be(1);
        group2.Value.members.Should().HaveCount(1);
        group2.Value.members[0].userId.Should().Be(player.PersistentId);
    }

    //[Fact]
    public async Task YouCanAddMemberToGroupAndThenRemoveIt()
    {
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();

        var groupService = server.GetRequiredService<IGroupService>();
        var group = await groupService.CreateGroup("Test group4", "TG4", GroupKind.Regular);

        await groupService.TryAddMember(player, group.id, 100, "Leader");
        var removed = await groupService.RemoveMember(player, player.PersistentId);

        player.Groups.IsMember(group.id).Should().BeFalse();
        var group2 = await groupService.GetGroupByName("Test group4");
        removed.Should().BeTrue();
        group2.Value.members.Should().BeEmpty();
    }
}
