using RealmCore.Server.Concepts.Interfaces;

namespace RealmCore.Tests.Tests;

public class SaveServiceTests
{
    [Fact]
    public async Task SpawningTwoPersistantVehiclesShouldNotBeAllowed()
    {
        var realmTestingServer = new RealmTestingServer();
        var vehiclesService = realmTestingServer.GetRequiredService<IVehiclesService>();
        var loadService = realmTestingServer.GetRequiredService<ILoadService>();
        var saveService = realmTestingServer.GetRequiredService<ISaveService>();
        var vehicle = await vehiclesService.CreatePersistantVehicle(404, Vector3.Zero, Vector3.Zero);
        var id = vehicle.PersistantId;
        await saveService.Save(vehicle);
        vehicle.Destroy();

        var spawn = async () => await loadService.LoadVehicleById(id);
        spawn.Should().NotThrow();
        spawn.Should().Throw<PersistantVehicleAlreadySpawnedException>().WithMessage("Failed to create already existing vehicle.");
    }

    [Fact]
    public async Task SpawnedVehicleShouldBeExactlyTheSameAsSavedOne()
    {
        var realmTestingServer = new RealmTestingServer();
        realmTestingServer.GetRequiredService<VehicleUpgradeRegistry>().AddUpgrade(250, EmptyVehicleHandlingModifier.Instance);
        var player = realmTestingServer.CreatePlayer();
        await realmTestingServer.SignInPlayer(player);
        var vehiclesService = realmTestingServer.GetRequiredService<IVehiclesService>();
        var loadService = realmTestingServer.GetRequiredService<ILoadService>();
        var saveService = realmTestingServer.GetRequiredService<ISaveService>();
        var vehicle1 = await vehiclesService.CreatePersistantVehicle(404, new Vector3(1,2,3), new Vector3(4, 5, 6));
        vehicle1.Access.AddAsOwner(player);
        vehicle1.MileageCounter.Mileage = 123;
        vehicle1.Upgrades.AddUpgrade(250, false);
        vehicle1.PartDamage.AddPart(200, 300);
        vehicle1.Engines.Add(50);
        var id = vehicle1.PersistantId;
        await saveService.Save(vehicle1);
        vehicle1.Destroy();

        var vehicle2 = await loadService.LoadVehicleById(id);

        vehicle1.Access.IsOwner(player).Should().BeTrue();
        vehicle2.MileageCounter.Mileage.Should().Be(123);
        vehicle2.Upgrades.Should().BeEquivalentTo([250]);
        vehicle2.PartDamage.Parts.Should().BeEquivalentTo([200]);
        vehicle2.PartDamage.GetState(200).Should().Be(300);
        vehicle2.Engines.EnginesIds.Should().BeEquivalentTo([50]);

        // TODO: use extension method
        vehicle1.Should().BeEquivalentTo(vehicle2, x =>
        {
            x.Including(y => y.Position);
            x.Including(y => y.PaintJob);
            return x;
        });
    }
}
