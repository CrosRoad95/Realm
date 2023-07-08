namespace RealmCore.Server.Commands;

internal class EssentialCommandsLogic
{
    private readonly IConsole _consoleCommands;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EssentialCommandsLogic> _logger;
    private readonly Dictionary<string, Type> _commands = new();

    public EssentialCommandsLogic(IConsole consoleCommands, IEnumerable<CommandTypeWrapper> commands,
        IServiceProvider serviceProvider, ILogger<EssentialCommandsLogic> logger)
    {
        _consoleCommands = consoleCommands;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _consoleCommands.CommandExecuted += HandleCommandExecuted;

        _commands["help"] = typeof(HelpCommand);
        foreach (var item in commands)
        {
            var commandNameAttribute = item.Type.GetCustomAttribute<CommandNameAttribute>();
            if (commandNameAttribute == null)
            {
                logger.LogWarning("Command class {className} has no CommandName attribute", item.Type.Name);
                continue;
            }

            var commandName = commandNameAttribute.Name.ToLower();
            if (_commands.ContainsKey(commandName))
            {
                _commands.Clear();
                throw new Exception($"Command '{commandName}' already exists");
            }
            _commands[commandName] = item.Type;
        }
    }

    private void HandleCommandExecuted(Entity entity, string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

        var firstWord = line.Split(' ').FirstOrDefault();
        if (firstWord == null)
            return;
        var loweredFirstWord = firstWord.ToLower();

        if (_commands.TryGetValue(loweredFirstWord, out Type? value))
        {
            try
            {
                var command = _serviceProvider.GetRequiredService(value) as ICommand;
                if (command == null)
                    throw new InvalidOperationException("Expected command to implement ICommand interface");
                command.Handle(entity, line.Split(' ').ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute command: '{commandName}' not found", loweredFirstWord);
            }
        }
        else
        {
            _logger.LogWarning("Command '{commandName}' not found", loweredFirstWord);
        }
    }
}
