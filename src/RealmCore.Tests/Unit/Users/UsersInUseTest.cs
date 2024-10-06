namespace RealmCore.Tests.Unit.Users;

public class UsersInUseTest
{
    [Fact]
    public void UsersInUseShouldWork()
    {
        var activeUsers = new UsersInUse();

        var preIsActive = activeUsers.IsActive(1);
        bool set1 = activeUsers.TrySetActive(1, null);
        bool set2 = activeUsers.TrySetActive(1, null);
        var innerIsActive = activeUsers.IsActive(1);
        var set = activeUsers.ActiveUsersIds;
        var set3 = activeUsers.TrySetInactive(1);
        var set4 = activeUsers.TrySetInactive(1);
        var afterIsActive = activeUsers.IsActive(1);

        preIsActive.Should().BeFalse();
        set1.Should().BeTrue();
        set2.Should().BeFalse();
        innerIsActive.Should().BeTrue();
        set.Should().BeEquivalentTo([1]);
        set3.Should().BeTrue();
        set4.Should().BeFalse();
        afterIsActive.Should().BeFalse();
    }
}
