namespace RealmCore.Server.Modules.Domain;

public class AtomicBool
{
    private int _value; // 0 for false, 1 for true

    public AtomicBool(bool initialValue = false)
    {
        _value = initialValue ? 1 : 0;
    }

    public bool TrySetTrue()
    {
        return Interlocked.Exchange(ref _value, 1) == 0;
    }

    public bool TrySetFalse()
    {
        return Interlocked.Exchange(ref _value, 0) == 1;
    }

    public static implicit operator bool(AtomicBool atomicBool)
    {
        return atomicBool._value == 1 ? true : false;
    }

    public static implicit operator AtomicBool(bool value)
    {
        return new AtomicBool(value);
    }

    public override string ToString() => _value == 1 ? "true" : "false";
}
