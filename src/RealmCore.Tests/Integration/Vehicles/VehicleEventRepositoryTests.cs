namespace RealmCore.Tests.Integration.Vehicles;

[Collection("IntegrationTests")]
public class VehicleEventRepositoryTests : RealmRemoteDatabaseIntegrationTestingBase
{
    [Fact]
    public async Task VehicleEventRepositoryShouldWork()
    {
        var server = await CreateServerAsync();
        var player = await CreatePlayerAsync();
        var vehicle1 = await CreateVehicleAsync();
        var vehicle2 = await CreateVehicleAsync();
        var vehicleId1 = vehicle1.VehicleId;
        var vehicleId2 = vehicle2.VehicleId;

        var dateTimeProvider = server.GetRequiredService<IDateTimeProvider>();
        var vehicleEventRepository = server.GetRequiredService<IVehicleEventRepository>();

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
