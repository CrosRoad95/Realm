using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Realm.Domain.Exceptions;
using Realm.Persistance;
using Realm.Persistance.Interfaces;
using Realm.Persistance.Repository;
using Realm.Persistance.SQLite;
using Realm.Server.Interfaces;
using Realm.Server.Services;

namespace Realm.Tests.Tests;

public class GroupServiceTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    public GroupServiceTests()
    {
        var services = new ServiceCollection();
        services.AddPersistance<SQLiteDb>(db => db.UseInMemoryDatabase("inMemoryDatabase"));
        services.AddTransient<IGroupRepository, GroupRepository>();
        services.AddTransient<IGroupService, GroupService>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task GroupShouldBePossibleToCreate()
    {
        const string groupName = "Test group";
        var groupService = _serviceProvider.GetRequiredService<IGroupService>();

        await groupService.CreateGroup(groupName, "TG", Domain.Enums.GroupKind.Regular);

        var group = await groupService.GetGroupByName(groupName);

        group.Value.name.Should().Be(groupName);
    }

    [Fact]
    public async Task YouCanNotCreateTwoGroupsWithTheSameName()
    {
        const string groupName = "Test group";
        var groupService = _serviceProvider.GetRequiredService<IGroupService>();

        Func<Task> createGroup = async () => await groupService.CreateGroup(groupName, "TG", Domain.Enums.GroupKind.Regular);

        await createGroup.Should().NotThrowAsync();
        (await createGroup.Should().ThrowAsync<GroupNameInUseException>())
            .WithMessage($"Group '{groupName}' is already in use");
    }

    [Fact]
    public async Task YouCanAddMemberToGroup()
    {
        var userGuid = Guid.NewGuid();
        var groupService = _serviceProvider.GetRequiredService<IGroupService>();
        var newlyCreatedGroup = await groupService.CreateGroup("Test group", "TG", Domain.Enums.GroupKind.Regular);

        await groupService.AddMember(newlyCreatedGroup.id, userGuid, 100, "Leader");

        var group = await groupService.GetGroupByName("Test group");
        group.Value.members.Should().HaveCount(1);
        group.Value.members[0].userId.Should().Be(userGuid);
    }

    [Fact]
    public async Task YouCanAddMemberToGroupAndThenRemoveIt()
    {
        var userGuid = Guid.NewGuid();
        var groupService = _serviceProvider.GetRequiredService<IGroupService>();
        var newlyCreatedGroup = await groupService.CreateGroup("Test group", "TG", Domain.Enums.GroupKind.Regular);

        await groupService.AddMember(newlyCreatedGroup.id, userGuid, 100, "Leader");
        await groupService.RemoveMember(newlyCreatedGroup.id, userGuid);

        var group = await groupService.GetGroupByName("Test group");
        group.Value.members.Should().BeEmpty();
    }

    public void Dispose()
    {
        var db = _serviceProvider.GetRequiredService<IDb>();
        db.GroupMembers.RemoveRange(db.GroupMembers);
        db.Groups.RemoveRange(db.Groups);
        db.SaveChangesAsync().Wait();
    }
}
