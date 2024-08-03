namespace RealmCore.BlazorGui.Commands;

[CommandName("givereward")]
public sealed class GiveRewardCommand : IInGameCommand
{
    private readonly ILogger<GiveRewardCommand> _logger;
    private readonly RewardsService _rewardService;
    private readonly ChatBox _chatBox;

    public string[] RequiredPolicies { get; } = [];
    public GiveRewardCommand(ILogger<GiveRewardCommand> logger, RewardsService rewardService, ChatBox chatBox)
    {
        _logger = logger;
        _rewardService = rewardService;
        _chatBox = chatBox;
    }

    public async Task Handle(RealmPlayer player, CommandArguments args, CancellationToken cancellationToken)
    {
        if (await _rewardService.TryGiveReward(player, 1, cancellationToken))
        {
            _chatBox.OutputTo(player, "Nagroda id 1 została odebrana pomyślnie");
        }
        else
        {
            _chatBox.OutputTo(player, "Już otrzymałeś nagrode id 1");
        }
    }
}