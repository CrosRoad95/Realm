
using System.Globalization;

namespace RealmCore.Tests;

public class RealmTestingPlayer : RealmPlayer
{
    public override Vector2 ScreenSize { get; internal set; } = new Vector2(1920, 1080);
    public override CultureInfo CultureInfo { get; internal set; } = new CultureInfo("pl-PL");

    public RealmTestingPlayer(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}
