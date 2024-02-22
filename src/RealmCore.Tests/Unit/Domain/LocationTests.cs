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
}
