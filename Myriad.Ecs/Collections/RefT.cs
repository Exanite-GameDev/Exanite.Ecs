namespace Myriad.Ecs.Collections;

public readonly ref struct Ref<T>
{
    public readonly ref T Value;

    public Ref(ref T r)
    {
        Value = ref r;
    }
}
