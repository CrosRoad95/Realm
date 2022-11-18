using Serilog.Context;
using SlipeServer.Server.Elements.ColShapes;

namespace Realm.Server.Elements.CollisionShapes;

public class RPGCollisionSphere : CollisionSphere, IDisposable
{
    private bool _disposed;
    private readonly bool _isPersistant = PersistantScope.IsPersistant;
    private readonly ILogger _logger;
    private ScriptObject? _elementEntered;

    public bool IsVariant { get; private set; }
    public RPGCollisionSphere() : base(Vector3.Zero, 0)
    {
        ElementEntered += CollisionShape_ElementEntered;
    }

    [ScriptMember("onEnter")]
    public bool onEnter(ScriptObject onUsedCallback)
    {
        CheckIfDisposed();
        _elementEntered = onUsedCallback;
        return true;
    }

    private async void CollisionShape_ElementEntered(Element element)
    {
        if (_elementEntered == null)
            return;

        try
        {
            if (_elementEntered.IsAsync())
                await (_elementEntered.Invoke(false, element) as dynamic);
            else
                _elementEntered.Invoke(false, element);
        }
        catch (ScriptEngineException scriptEngineException)
        {
            var scriptException = scriptEngineException as IScriptEngineException;
            if (scriptException != null)
            {
                using var errorDetails = LogContext.PushProperty("errorDetails", scriptException.ErrorDetails);
                _logger.Error(scriptEngineException, "Exception thrown while executing event");
            }
            else
                _logger.Error(scriptEngineException, "Exception thrown while executing event");
        }
    }

    public void SetIsVariant()
    {
        CheckIfDisposed();
        IsVariant = true;
    }

    [ScriptMember("isPersistant")]
    public bool IsPersistant()
    {
        CheckIfDisposed();
        return _isPersistant;
    }

    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
