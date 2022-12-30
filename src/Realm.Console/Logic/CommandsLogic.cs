using Realm.Domain;
using Realm.Domain.Components.Common;
using Realm.Server.Services;
using Serilog;

namespace Realm.Console.Logic;

internal sealed class CommandsLogic
{
    private readonly RPGCommandService _commandService;
    private readonly ILogger _logger;

    public CommandsLogic(RPGCommandService commandService, ILogger logger)
    {
        _commandService = commandService;
        _logger = logger;
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

    }
}
