using Realm.Domain.Components.Players;

namespace Realm.Console.Logic;

internal sealed class CommandsLogic
{
    private readonly RPGCommandService _commandService;
    private readonly ILogger _logger;
    private readonly IEntityFactory _entityFactory;
    private readonly ILoadAndSaveService _loadAndSaveService;
    private readonly RepositoryFactory _repositoryFactory;

    public CommandsLogic(RPGCommandService commandService, ILogger logger, IEntityFactory entityFactory, ILoadAndSaveService loadAndSaveService, RepositoryFactory repositoryFactory)
    {
        _commandService = commandService;
        _logger = logger;
        _entityFactory = entityFactory;
        _loadAndSaveService = loadAndSaveService;
        _repositoryFactory = repositoryFactory;
        _commandService.AddCommandHandler("gp", (entity, args) =>
        {
            logger.Information("{position}, {rotation}", entity.Transform.Position, entity.Transform.Rotation);
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("inventory", (entity, args) =>
        {
            if (entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                playerElementComponent.SendChatMessage($"Inventory, {inventoryComponent.Number}/{inventoryComponent.Size}");
                foreach (var item in inventoryComponent.Items)
                {
                    playerElementComponent.SendChatMessage($"Item, {item.Id} = {item.Name}");
                }

            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("giveitem", (entity, args) =>
        {
            if(entity.TryGetComponent(out InventoryComponent inventoryComponent))
            {
                uint itemId = uint.Parse(args.FirstOrDefault("1"));
                if(inventoryComponent.AddItem(itemId) != null)
                {
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"Item added, {inventoryComponent.Number}/{inventoryComponent.Size}");
                }
            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("licenses", (entity, args) =>
        {
            if (entity.TryGetComponent(out LicensesComponent licenseComponent))
            {
                var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
                playerElementComponent.SendChatMessage($"Licenses");
                foreach (var license in licenseComponent.Licenses)
                {
                    playerElementComponent.SendChatMessage($"License: {license.LicenseId} = {license.IsSuspended()}");
                }

            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("givelicense", (entity, args) =>
        {
            if (entity.TryGetComponent(out LicensesComponent licenseComponent))
            {
                var license = args.First();
                if (licenseComponent.AddLicense(license))
                {
                    entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"license added: {license}");
                }
            }
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("playtime", (entity, args) =>
        {
            if (entity.TryGetComponent(out PlayTimeComponent playTimeComponent))
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"playtime: {playTimeComponent.PlayTime}, total play time: {playTimeComponent.TotalPlayTime}");
            }
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("givemoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = decimal.Parse(args.First());
                moneyComponent.GiveMoney(amount);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"gave money: {amount}, total money: {moneyComponent.Money}");
            }
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("takemoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = decimal.Parse(args.First());
                moneyComponent.TakeMoney(amount);
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"taken money: {amount}, total money: {moneyComponent.Money}");
            }
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("setmoney", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                var amount = decimal.Parse(args.First());
                moneyComponent.Money = amount;
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"taken money: {amount}, total money: {moneyComponent.Money}");
            }
            return Task.CompletedTask;
        });
        
        _commandService.AddCommandHandler("money", (entity, args) =>
        {
            if (entity.TryGetComponent(out MoneyComponent moneyComponent))
            {
                entity.GetRequiredComponent<PlayerElementComponent>().SendChatMessage($"total money: {moneyComponent.Money}");
            }
            return Task.CompletedTask;
        });

        _commandService.AddCommandHandler("cv", async (entity, args) =>
        {
            using var vehicleRepository = _repositoryFactory.GetVehicleRepository();
            var vehicleEntity = _entityFactory.CreateVehicle(404, entity.Transform.Position + new Vector3(4, 0, 0), entity.Transform.Rotation);
            vehicleEntity.AddComponent(new PrivateVehicleComponent(await vehicleRepository.CreateNewVehicle()));
        });

        _commandService.AddCommandHandler("addmeasowner", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            veh.GetRequiredComponent<PrivateVehicleComponent>().AddOwner(entity);
        });

        _commandService.AddCommandHandler("accessinfo", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var veh = playerElementComponent.OccupiedVehicle;
            if(veh == null)
            {
                playerElementComponent.SendChatMessage("Enter vehicle!");
                return;
            }

            var privateVehicleComponent = veh.GetRequiredComponent<PrivateVehicleComponent>();
            playerElementComponent.SendChatMessage("Access info:");

            foreach (var vehicleAccess in privateVehicleComponent.VehicleAccesses)
            {
                playerElementComponent.SendChatMessage($"Access: ({vehicleAccess.UserId}) = Ownership={vehicleAccess.Ownership}");
            }
        });
    }
}
