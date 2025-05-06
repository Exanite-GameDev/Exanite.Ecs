namespace Myriad.Ecs.Collections;

public readonly ref struct RefT<T>
{
    public readonly ref T Value;

    public RefT(ref T r)
    {
        Value = ref r;
    }
}
