using RealmCore.Server.Services;

namespace RealmCore.Console.Extra;

internal class ExtraCommandsLogic
{
    private readonly IDiscordService _discordService;
    private readonly ChatBox _chatBox;
    private readonly IElementCollection _elementCollection;

    public ExtraCommandsLogic(IDiscordService discordService, RealmCommandService _commandService, ChatBox chatBox, IElementCollection elementCollection)
    {
        _discordService = discordService;
        _chatBox = chatBox;
        _elementCollection = elementCollection;
        _discordService.AddTextBasedCommandHandler(1069962155539042314, "test", (userId, parameters) =>
        {
            _chatBox.Output($"Użytkownik o id {userId} wpisał komendę 'test' z parametrami: {parameters}");
            return Task.CompletedTask;
        });

        _discordService.AddTextBasedCommandHandler(997787973775011853, "gracze", async (userId, parameters) =>
        {
            var players = _elementCollection.GetByType<RealmPlayer>();
            await _discordService.SendMessage(997787973775011853, $"Gracze na serwerze: {string.Join(", ", players.Select(x => x.Name))}");
        });

        _commandService.AddAsyncCommandHandler("discordsendmessage", async (player, args, token) =>
        {
            var messageId = await _discordService.SendMessage(1079342213097607399, args.ReadAllAsString(), token);
            _chatBox.OutputTo(player, $"Wysłano wiadomość, id: {messageId}");
        });

        _commandService.AddAsyncCommandHandler("discordsendmessagetouser", async (player, args, token) =>
        {
            var messageId = await _discordService.SendMessageToUser(659910279353729086, args.ReadAllAsString(), token);
            _chatBox.OutputTo(player, $"Wysłano wiadomość, id: {messageId}");
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

        _commandService.AddAsyncCommandHandler("discordsendfile", async (player, args, token) =>
        {
            var messageId = await _discordService.SendFile(997787973775011853, generateStreamFromString("dowody"), "dowody_na_borsuka.txt", "potwierdzam", token);
            _chatBox.OutputTo(player, $"Wysłano plik, id: {messageId}");
        });


    }
}
