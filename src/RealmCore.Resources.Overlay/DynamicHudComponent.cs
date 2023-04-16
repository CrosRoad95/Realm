using System.Reflection;

namespace RealmCore.Resources.Overlay;

public class DynamicHudComponent
{
    public int ComponentId { get; set; }
    public PropertyInfo PropertyInfo { get; set; }
    public dynamic Factory { get; set; }
}
