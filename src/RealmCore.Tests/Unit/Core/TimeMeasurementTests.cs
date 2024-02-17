namespace RealmCore.Tests.Unit.Core;

public class TimeMeasurementTests
{
    [Fact]
    public void TimeMeasurementShouldWork()
    {
        var dateTimeProvider = new TestDateTimeProvider();
        var timeMeasurement = new TimeMeasurement(dateTimeProvider);
        timeMeasurement.TryStart();

        dateTimeProvider.AddOffset(TimeSpan.FromSeconds(10));

        timeMeasurement.TryStop();

        timeMeasurement.Elapsed.Should().Be(TimeSpan.FromSeconds(10));
    }
}
