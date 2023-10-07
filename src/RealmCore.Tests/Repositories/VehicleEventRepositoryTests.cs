namespace RealmCore.Tests.Repositories;

public class VehicleEventRepositoryTests
{
    private readonly RealmTestingServer _server;
    public VehicleEventRepositoryTests()
    {
        _server = new();
    }

    [Fact]
    public async Task VehicleEventRepositoryShouldWork()
    {
        var dateTimeProvider = _server.GetRequiredService<IDateTimeProvider>();
        var vehicleEventRepository = _server.GetRequiredService<IVehicleEventRepository>();
        
        await vehicleEventRepository.AddEvent(1, 1, dateTimeProvider.Now, "a");
        await vehicleEventRepository.AddEvent(1, 1, dateTimeProvider.Now.AddMinutes(1), "b");
        await vehicleEventRepository.AddEvent(1, 2, dateTimeProvider.Now.AddMinutes(2), "c");
        await vehicleEventRepository.AddEvent(1, 2, dateTimeProvider.Now.AddMinutes(3), "d");
        await vehicleEventRepository.AddEvent(2, 2, dateTimeProvider.Now, "d");

        var events = await vehicleEventRepository.GetAllEventsByVehicleId(1);
        events.Should().HaveCount(4);
        events = await vehicleEventRepository.GetAllEventsByVehicleId(1, new int[] { 1 });
        events.Should().HaveCount(2);

        events = await vehicleEventRepository.GetLastEventsByVehicleId(1, 2);
        events.Select(x => x.Metadata).Should().BeEquivalentTo(new string[] { "c", "d" });
    }
}
