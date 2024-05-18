namespace RealmCore.Tests.Integration.Players;

[Collection("IntegrationTests")]
public class GroupServiceTests : RealmRemoteDatabaseIntegrationTestingBase
{
    [Fact]
    public async Task GroupShouldBePossibleToCreate()
    {
        var server = await CreateServerAsync();

        var groupService = server.GetRequiredService<IGroupsService>();

        var groupName = Guid.NewGuid().ToString();

        await groupService.CreateGroup(groupName, groupName[..8], GroupKind.Regular);

        var group = await groupService.GetGroupByName(groupName);

        group.Value.name.Should().Be(groupName);
    }

    //[Fact]
    public async Task YouCanNotCreateTwoGroupsWithTheSameName()
    {
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();

        var groupService = server.GetRequiredService<IGroupsService>();

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

        var groupService = server.GetRequiredService<IGroupsService>();

        var groupName = Guid.NewGuid().ToString();
        var group = await groupService.CreateGroup(groupName, groupName[..8], GroupKind.Regular);

        await groupService.TryAddMember(player, group.id, 1, "Leader");

        var member = player.Groups.GetMemberOrDefault(group.id) ?? throw new InvalidOperationException();
        var group2 = await groupService.GetGroupByName(groupName);
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

        var groupService = server.GetRequiredService<IGroupsService>();
        var group = await groupService.CreateGroup("Test group4", "TG4", GroupKind.Regular);

        await groupService.TryAddMember(player, group.id, 100, "Leader");
        var removed = await groupService.RemoveMember(player, player.PersistentId);

        player.Groups.IsMember(group.id).Should().BeFalse();
        var group2 = await groupService.GetGroupByName("Test group4");
        removed.Should().BeTrue();
        group2.Value.members.Should().BeEmpty();
    }
}
