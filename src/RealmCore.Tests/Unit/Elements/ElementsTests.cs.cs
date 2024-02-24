namespace RealmCore.Tests.Unit.Elements;

public class ElementsTests
{
    [Fact]
    public void CancellationTokenTests()
    {
        var element = new Element();

        var token = element.CreateCancellationToken();

        token.IsCancellationRequested.Should().BeFalse();
        element.Destroy();
        token.IsCancellationRequested.Should().BeTrue();
    }
}
