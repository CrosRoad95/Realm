namespace RealmCore.Tests;

public class RealmTestingPlayer : RealmPlayer
{
    public override Vector2 ScreenSize { get; internal set; } = new Vector2(1920, 1080);

    public RealmTestingPlayer(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
