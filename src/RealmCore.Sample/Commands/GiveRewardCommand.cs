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

    public async Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        if (await _rewardService.TryGiveReward(player, 1))
        {
            _chatBox.OutputTo(player, "Nagroda id 1 została odebrana pomyślnie");
        }
        else
        {
            _chatBox.OutputTo(player, "Już otrzymałeś nagrode id 1");
        }
    }
}