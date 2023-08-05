using RealmCore.Persistence;
using RealmCore.Persistence.Interfaces;
using RealmCore.Persistence.Repository;

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
        var newlyCreatedGroup = await groupService.CreateGroup("Test group3", "TG3", GroupKind.Regular);

        await groupService.AddMember(newlyCreatedGroup.id, userId, 100, "Leader");

        var group = await groupService.GetGroupByName("Test group3");
        group.Value.members.Should().HaveCount(1);
        group.Value.members[0].userId.Should().Be(userId);
    }

    [Fact]
    public async Task YouCanAddMemberToGroupAndThenRemoveIt()
    {
        var userId = 1;
        var groupService = _serviceProvider.GetRequiredService<IGroupService>();
        var newlyCreatedGroup = await groupService.CreateGroup("Test group4", "TG4", GroupKind.Regular);

        await groupService.AddMember(newlyCreatedGroup.id, userId, 100, "Leader");
        await groupService.RemoveMember(newlyCreatedGroup.id, userId);

        var group = await groupService.GetGroupByName("Test group4");
        group.Value.members.Should().BeEmpty();
    }
}
