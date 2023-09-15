using RealmCore.Persistence.Data;
using RealmCore.Server.Components.TagComponents;
using RealmCore.SQLite;

namespace RealmCore.Tests.Tests;

public class GroupServiceTests
{
    private readonly IServiceProvider _serviceProvider;
    public GroupServiceTests()
    {
        var services = new ServiceCollection();
        services.AddPersistence<SQLiteDb>(db => db.UseInMemoryDatabase("inMemoryDatabase"), ServiceLifetime.Singleton);
        services.AddTransient<IGroupRepository, GroupRepository>();
        services.AddTransient<IGroupService, GroupService>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task GroupShouldBePossibleToCreate()
    {
        const string groupName = "Test group1";
        var groupService = _serviceProvider.GetRequiredService<IGroupService>();

        await groupService.CreateGroup(groupName, "TG1", GroupKind.Regular);

        var group = await groupService.GetGroupByName(groupName);

        group.Value.name.Should().Be(groupName);
    }

    [Fact]
    public async Task YouCanNotCreateTwoGroupsWithTheSameName()
    {
        const string groupName = "Test group2";
        var groupService = _serviceProvider.GetRequiredService<IGroupService>();

        Func<Task> createGroup = async () => await groupService.CreateGroup(groupName, "TG2", GroupKind.Regular);

        await createGroup.Should().NotThrowAsync();
        (await createGroup.Should().ThrowAsync<GroupNameInUseException>())
            .WithMessage($"Group '{groupName}' is already in use");
    }

    [Fact]
    public async Task YouCanAddMemberToGroup()
    {
        var userId = 1;
        var groupService = _serviceProvider.GetRequiredService<IGroupService>();
        var entity = new Entity();
        entity.AddComponent<PlayerTagComponent>();
        await entity.AddComponentAsync(new UserComponent(new UserData
        {
            Id = userId,
            Upgrades = new List<UserUpgradeData>()
        }, null, null));
        var group = await groupService.CreateGroup("Test group3", "TG3", GroupKind.Regular);

        await groupService.AddMember(entity, group.id, 1, "Leader");

        var groupMemberComponent = entity.GetRequiredComponent<GroupMemberComponent>();
        var group2 = await groupService.GetGroupByName("Test group3");
        groupMemberComponent.GroupId.Should().Be(group.id);
        groupMemberComponent.RankName.Should().Be("Leader");
        groupMemberComponent.Rank.Should().Be(1);
        group2.Value.members.Should().HaveCount(1);
        group2.Value.members[0].userId.Should().Be(userId);
    }

    //[Fact]
    public async Task YouCanAddMemberToGroupAndThenRemoveIt()
    {
        var userId = 1;
        var groupService = _serviceProvider.GetRequiredService<IGroupService>();
        var group = await groupService.CreateGroup("Test group4", "TG4", GroupKind.Regular);

        var entity = new Entity();
        entity.AddComponent<PlayerTagComponent>();
        await entity.AddComponentAsync(new UserComponent(new UserData
        {
            Id = userId,
            Upgrades = new List<UserUpgradeData>()
        }, null, null));

        await groupService.AddMember(entity, group.id, 100, "Leader");
        var removed = await groupService.RemoveMember(entity, userId);

        entity.HasComponent<GroupMemberComponent>().Should().BeFalse();
        var group2 = await groupService.GetGroupByName("Test group4");
        removed.Should().BeTrue();
        group2.Value.members.Should().BeEmpty();
    }
}
