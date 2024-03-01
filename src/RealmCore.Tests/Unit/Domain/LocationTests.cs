namespace RealmCore.Tests.Unit.Domain;

public class LocationTests
{
    [Fact]
    public void LocationShouldWorkAsExpected()
    {
        var location = new Location(new Vector3(1,2,3), new Vector3(1,2,3), 1, 2);

        var location2 = location with { Dimension = 3 };

        location2.Should().BeEquivalentTo(new Location(new Vector3(1, 2, 3), new Vector3(1, 2, 3), 1, 3));

        location2.ToString().Should().Be("Position: 1.00, 2.00, 3.00, Rotation: 1.00, 2.00, 3.00, Interior: 1, Dimension: 3");
    }

    [InlineData(0,0,0,0, true)]
    [InlineData(0,0,1,0, false)]
    [InlineData(0,0,0,1, false)]
    [Theory]
    public void CompareShouldWork(byte interior1, ushort dimension1, byte interior2, ushort dimension2, bool areEqual)
    {
        var location1 = new Location(new Vector3(1,2,3), new Vector3(1,2,3), interior1, dimension1);
        var location2 = new Location(new Vector3(1,2,3), new Vector3(1,2,3), interior2, dimension2);

        location1.CompareInteriorAndDimension(location2).Should().Be(areEqual);
    }

    [InlineData(0, 10)]
    [InlineData(1, float.MaxValue)]
    [Theory]
    public void DistanceShouldBeCalculatedProperly(byte interior, float expectedDistance)
    {
        var location1 = new Location(new Vector3(0,2,3), new Vector3(1,2,3), 0, 0);
        var location2 = new Location(new Vector3(10,2,3), new Vector3(1,2,3), interior, 0);

        location1.DistanceTo(location2).Should().Be(expectedDistance);
    }
}
