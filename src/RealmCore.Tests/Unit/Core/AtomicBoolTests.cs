namespace RealmCore.Tests.Unit.Core;

public class AtomicBoolTests
{
    [Fact]
    public void AtomicBoolShouldWork()
    {
        AtomicBool atomicBool = new(false);
        ((bool)atomicBool).Should().Be(false);

        atomicBool.TrySetTrue().Should().BeTrue();
        atomicBool.TrySetTrue().Should().BeFalse();
        ((bool)atomicBool).Should().Be(true);

        atomicBool.TrySetFalse().Should().BeTrue();
        atomicBool.TrySetFalse().Should().BeFalse();
        ((bool)atomicBool).Should().Be(false);
    }
}
