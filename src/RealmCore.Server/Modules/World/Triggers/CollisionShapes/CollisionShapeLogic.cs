namespace RealmCore.Server.Modules.World.Triggers.CollisionShapes;

internal sealed class CollisionShapeLogic
{
    private readonly IElementFactory _elementFactory;
    private readonly ILogger<CollisionShapeLogic> _logger;

    public CollisionShapeLogic(IElementFactory elementFactory, ILogger<CollisionShapeLogic> logger)
    {
        _elementFactory = elementFactory;
        _logger = logger;
        _elementFactory.ElementCreated += HandleElementCreated;
    }

    private void HandleElementCreated(IElementFactory elementFactory, Element element)
    {
        if (element is not RealmCollisionSphere sphere)
            return;

        void HandleElementEntered(Element element)
        {
            try
            {
                sphere.CollisionDetection.RelayEntered(element);
            }
            catch (Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        }

        void HandleElementLeft(Element element)
        {
            try
            {
                sphere.CollisionDetection.RelayLeft(element);
            }
            catch (Exception ex)
            {
                _logger.LogHandleError(ex);
            }
        }

        void HandleDestroyed(Element element)
        {
            var pickup = (RealmCollisionSphere)element;
            pickup.ElementEntered -= HandleElementEntered;
            pickup.ElementLeft -= HandleElementLeft;
            pickup.Destroyed -= HandleDestroyed;
        }

        sphere.ElementEntered += HandleElementEntered;
        sphere.ElementLeft += HandleElementLeft;
        sphere.Destroyed += HandleDestroyed;
    }
}
