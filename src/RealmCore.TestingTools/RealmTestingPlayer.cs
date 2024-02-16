namespace RealmCore.TestingTools;

public class RealmTestingPlayer : RealmPlayer
{
    public override Vector2 ScreenSize { get; set; } = new Vector2(1920, 1080);

    public RealmTestingPlayer(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
