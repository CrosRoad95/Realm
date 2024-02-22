namespace RealmCore.Tests.Unit.Elements;

public class ElementCustomDataFeatureTests
{
    private class MyCustomData1
    {
        public int Number { get; set; }
    }

    private class MyCustomData2
    {
        public int Number { get; set; }
    }

    [Fact]
    public void ElementCustomDataFeatureShouldWorkAsExpected()
    {
        var customDataFeature = new ElementCustomDataFeature();

        customDataFeature.Set(new MyCustomData1
        {
            Number = 123
        });

        customDataFeature.Get<MyCustomData1>()?.Number.Should().Be(123);

        customDataFeature.TryGet<MyCustomData2>(out var _).Should().BeFalse();
    }
}
