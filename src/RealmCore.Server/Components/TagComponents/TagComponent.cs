using RealmCore.ECS.Components;

namespace RealmCore.Server.Components.TagComponents;

[ComponentUsage]
public abstract class TagComponent : Component { }

public class PlayerTagComponent : TagComponent { }
public class PedTagComponent : TagComponent { }
public class VehicleTagComponent : TagComponent { }
public class BlipTagComponent : TagComponent { }
public class PickupTagComponent : TagComponent { }
public class MarkerTagComponent : TagComponent { }
public class CollisionShapeTagComponent : TagComponent { }
public class WorldObjectTagComponent : TagComponent { }
public class RadarAreaTagComponent : TagComponent { }
public class ConsoleTagComponent : TagComponent { }
