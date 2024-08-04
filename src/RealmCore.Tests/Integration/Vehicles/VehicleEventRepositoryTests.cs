namespace RealmCore.Tests.Integration.Vehicles;

public class VehicleEventRepositoryTests
{
    [Fact]
    public async Task VehicleEventRepositoryShouldWork()
    {
        using var hosting = new RealmTestingServerHosting();
        var player = await hosting.CreatePlayer();
        var vehicle1 = await hosting.CreatePersistentVehicle();
        var vehicle2 = await hosting.CreatePersistentVehicle();

        var vehicleId1 = vehicle1.VehicleId;
        var vehicleId2 = vehicle2.VehicleId;

        var dateTimeProvider = hosting.GetRequiredService<IDateTimeProvider>();
        var vehicleEventRepository = hosting.GetRequiredService<VehicleEventRepository>();

        await vehicleEventRepository.AddEvent(vehicleId1, 1, dateTimeProvider.Now, "a");
        await vehicleEventRepository.AddEvent(vehicleId1, 1, dateTimeProvider.Now.AddMinutes(1), "b");
        await vehicleEventRepository.AddEvent(vehicleId1, 2, dateTimeProvider.Now.AddMinutes(2), "c");
        await vehicleEventRepository.AddEvent(vehicleId1, 2, dateTimeProvider.Now.AddMinutes(3), "d");
        await vehicleEventRepository.AddEvent(vehicleId2, 2, dateTimeProvider.Now, "d");

        var events = await vehicleEventRepository.GetAllEventsByVehicleId(vehicleId1);
        events.Should().HaveCount(4);
        events = await vehicleEventRepository.GetAllEventsByVehicleId(vehicleId1, new int[] { 1 });
        events.Should().HaveCount(2);

        events = await vehicleEventRepository.GetLastEventsByVehicleId(vehicleId1, 2);
        events.Select(x => x.Metadata).Should().BeEquivalentTo(new string[] { "c", "d" });
    }
}
