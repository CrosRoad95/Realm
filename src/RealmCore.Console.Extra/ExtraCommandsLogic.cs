using RealmCore.Server;
using RealmCore.Server.Components.Elements;
using RealmCore.Server.Components.Elements.Abstractions;
using RealmCore.Server.Services;

namespace RealmCore.Console.Extra;

internal class ExtraCommandsLogic
{
    private readonly IDiscordService _discordService;
    private readonly ChatBox _chatBox;
    private readonly IEntityEngine _ecs;

    public ExtraCommandsLogic(IDiscordService discordService, RealmCommandService _commandService, ChatBox chatBox, IEntityEngine ecs)
    {
        _discordService = discordService;
        _chatBox = chatBox;
        _ecs = ecs;

        _discordService.AddTextBasedCommandHandler(1069962155539042314, "test", (userId, parameters) =>
        {
            _chatBox.Output($"Użytkownik o id {userId} wpisał komendę 'test' z parametrami: {parameters}");
            return Task.CompletedTask;
        });

        _discordService.AddTextBasedCommandHandler(997787973775011853, "gracze", async (userId, parameters) =>
        {
            var playerEntities = _ecs.PlayerEntities;
            await _discordService.SendMessage(997787973775011853, $"Gracze na serwerze: {string.Join(", ", playerEntities.Select(x => x.GetRequiredComponent<PlayerElementComponent>().Name))}");
        });

        _commandService.AddAsyncCommandHandler("discordsendmessage", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var messageId = await _discordService.SendMessage(1079342213097607399, args.ReadAllAsString());
            _chatBox.OutputTo(RealmInternal.GetPlayer(entity.GetRequiredComponent<ElementComponent>()), $"Wysłano wiadomość, id: {messageId}");
        });

        _commandService.AddAsyncCommandHandler("discordsendmessagetouser", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var messageId = await _discordService.SendMessageToUser(659910279353729086, args.ReadAllAsString());
            _chatBox.OutputTo(RealmInternal.GetPlayer(entity.GetRequiredComponent<ElementComponent>()), $"Wysłano wiadomość, id: {messageId}");
        });

        Stream generateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        _commandService.AddAsyncCommandHandler("discordsendfile", async (entity, args) =>
        {
            var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
            var messageId = await _discordService.SendFile(997787973775011853, generateStreamFromString("dowody"), "dowody_na_borsuka.txt", "potwierdzam");
            _chatBox.OutputTo(RealmInternal.GetPlayer(entity.GetRequiredComponent<ElementComponent>()), $"Wysłano plik, id: {messageId}");
        });


    }
}
