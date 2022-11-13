public class RPGRadarArea : RadarArea
{
    public bool IsVariant { get; private set; }
    public RPGRadarArea() : base(Vector2.Zero, Vector2.Zero, Color.Transparent)
    {
    }

    [NoScriptAccess]
    public void SetIsVariant()
    {
        IsVariant = true;
    }
}
