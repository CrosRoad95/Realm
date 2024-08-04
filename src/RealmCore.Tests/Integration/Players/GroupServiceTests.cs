namespace RealmCore.Tests.Integration.Players;

public class GroupServiceTests
{
    ////[Fact]
    //public async Task YouCanAddMemberToGroupAndThenRemoveIt()
    //{
    //    using var hosting = new RealmTestingServerHosting();
    //    var player = await hosting.CreatePlayer();

    //    var groupService = hosting.GetRequiredService<GroupsService>();
    //    var group = await groupService.CreateGroup("Test group4", "TG4", 0);

    //    await groupService.TryAddMember(player, group.id, 100, "Leader");
    //    var removed = await groupService.RemoveMember(player, player.UserId);

    //    player.Groups.IsMember(group.id).Should().BeFalse();
    //    var group2 = await groupService.GetGroupByName("Test group4");
    //    removed.Should().BeTrue();
    //    group2.Value.members.Should().BeEmpty();
    //}
}
