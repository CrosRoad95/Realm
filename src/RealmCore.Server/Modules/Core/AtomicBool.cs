namespace RealmCore.Server.Modules.Core;

public struct AtomicBool
{
    private int _value; // 0 for false, 1 for true

    public bool Value
    {
        readonly get { return _value == 1; }
        set { _value = value ? 1 : 0; }
    }

    public AtomicBool(bool initialValue = false)
    {
        Value = initialValue;
    }

    public bool CompareExchange(bool newValue, bool comparand)
    {
        int newIntValue = newValue ? 1 : 0;
        int comparandIntValue = comparand ? 1 : 0;

        return Interlocked.CompareExchange(ref _value, newIntValue, comparandIntValue) == comparandIntValue;
    }

    public bool TrySetTrue()
    {
        return CompareExchange(true, false);
    }

    public bool TrySetFalse()
    {
        return CompareExchange(false, true);
    }

    public static implicit operator bool(AtomicBool atomicBool)
    {
        return atomicBool.Value;
    }

    public static implicit operator AtomicBool(bool value)
    {
        return new AtomicBool(value);
    }
}
