﻿using Microsoft.Extensions.Logging;
using RealmCore.Server.Components.Elements;
using RealmCore.Server.Entities;
using RealmCore.Server.Factories;
using SlipeServer.Server.ElementCollections;
using System.Drawing;
using System.Numerics;

namespace RealmCore.Tests.Tests.Services;

public class SaveLoadServiceTests
{
    private readonly IServiceProvider _services;
    private readonly Mock<ILogger<LoadService>> _loggerLoadServiceMock = new(MockBehavior.Strict);
    private readonly Mock<ILogger<ECS>> _loggerECSMock = new(MockBehavior.Strict);
    private readonly Mock<IRPGServer> _rpgServerMock = new(MockBehavior.Strict);
    private readonly Mock<IElementCollection> _elementCollection = new(MockBehavior.Strict);
    private readonly Mock<VehicleUpgradeRegistry> _vehicleUpgradeRegistry = new(MockBehavior.Strict);

    public SaveLoadServiceTests()
    {
        _loggerLoadServiceMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Verifiable();

        _loggerECSMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Verifiable();

        _rpgServerMock.Setup(x => x.IsReady).Returns(true);
        _rpgServerMock.Setup(x => x.AssociateElement(It.IsAny<object>()));

        var services = new ServiceCollection();
        services.AddPersistance<SQLiteDb>(db => db.UseSqlite($"Filename=./{Path.GetRandomFileName().Replace(".", "")}.db"));
        services.AddSingleton<ISaveService, SaveService>();
        services.AddSingleton<ILoadService, LoadService>();
        services.AddSingleton<RepositoryFactory>();
        services.AddSingleton<IEntityFactory, EntityFactory>();
        services.AddSingleton<IVehicleRepository, VehicleRepository>();
        services.AddSingleton<IECS, ECS>();
        services.AddSingleton<IVehiclesService, VehiclesService>();
        services.AddSingleton(_elementCollection.Object);
        services.AddSingleton(_loggerLoadServiceMock.Object);
        services.AddSingleton(_loggerECSMock.Object);
        services.AddSingleton(_rpgServerMock.Object);
        services.AddSingleton(_vehicleUpgradeRegistry.Object);
        services.AddSingleton<IDateTimeProvider>(new TestDateTimeProvider());
        services.AddSingleton(new ItemsRegistry());
        _services = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Test()
    {
        #region Arrange
        await _services.GetRequiredService<IDb>().MigrateAsync();
        var entityFactory = _services.GetRequiredService<IEntityFactory>();
        var vehiclesService = _services.GetRequiredService<IVehiclesService>();
        var loadService = _services.GetRequiredService<ILoadService>();

        var vehicleEntity = await entityFactory.CreateNewPrivateVehicle(404, new Vector3(1, 2, 3), new Vector3(4, 5, 6));
        var vehicleElementComponent = vehicleEntity.GetRequiredComponent<VehicleElementComponent>();
        vehicleEntity.Transform.Position = new Vector3(10, 20, 30);
        vehicleEntity.Transform.Rotation = new Vector3(40, 50, 60);
        vehicleElementComponent.Health = 777;
        vehicleElementComponent.PrimaryColor = Color.LightGoldenrodYellow;
        vehicleElementComponent.SecondaryColor = Color.AntiqueWhite;
        vehicleElementComponent.Color3 = Color.Bisque;
        vehicleElementComponent.Color4 = Color.DarkKhaki;
        vehicleElementComponent.SetDoorState(SlipeServer.Packets.Enums.VehicleDoor.Trunk, SlipeServer.Packets.Enums.VehicleDoorState.ShutDamaged);
        #endregion

        #region Act
        var vehicle = vehicleElementComponent.Vehicle;
        await vehiclesService.Despawn(vehicleEntity);
        vehicle.IsDestroyed.Should().BeTrue();

        var loadedVehicleEntity = await loadService.LoadVehicleById(1);
        var loadedVehicleElementComponent = loadedVehicleEntity.GetRequiredComponent<VehicleElementComponent>();
        #endregion

        #region Assert
        loadedVehicleElementComponent.Vehicle.Should().BeEquivalentTo(vehicle, options => options
            .Excluding(x => x.Damage) // TODO: hack
            .Excluding(x => x.TimeContext)
            .Excluding(x => x.IsDestroyed)
            .Excluding(x => x.RespawnPosition)
            .Excluding(x => x.RespawnRotation));
        #endregion
    }
}