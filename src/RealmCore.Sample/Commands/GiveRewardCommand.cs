namespace RealmCore.Sample.Commands;


[CommandName("givereward")]
public sealed class GiveRewardCommand : IInGameCommand
{
    private readonly ILogger<GiveRewardCommand> _logger;
    private readonly IRewardService _rewardService;
    private readonly ChatBox _chatBox;

    public GiveRewardCommand(ILogger<GiveRewardCommand> logger, IRewardService rewardService, ChatBox chatBox)
    {
        _logger = logger;
        _rewardService = rewardService;
        _chatBox = chatBox;
    }

    public async Task Handle(Entity entity, CommandArguments args)
    {
        if (await _rewardService.TryGiveReward(entity, 1))
        {
            _chatBox.OutputTo(entity, "Nagroda id 1 została odebrana pomyślnie");
        }
        else
        {
            _chatBox.OutputTo(entity, "Już otrzymałeś nagrode id 1");
        }
    }
}