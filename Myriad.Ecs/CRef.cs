using Exanite.Core.Runtime;
using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs;

public readonly record struct CRef<T> where T : IComponent
{
    public readonly Entity Entity;

    /// <summary>
    /// Returns a mutable reference to the component data.
    /// <br/>
    /// Will throw an exception if the Entity is not alive or if the component does not exist.
    /// If <see cref="IsAlive"/> is <see langword="true"/>, then accessing this property is safe.
    /// </summary>
    public ref T Value => ref Entity.GetComponent<T>();

    /// <summary>
    /// Is the component alive? If <see langword="false"/>, accessing <see cref="Value"/> will throw an exception.
    /// </summary>
    public bool IsAlive => Entity.IsAlive && Entity.HasComponent<T>();

    internal CRef(Entity entity)
    {
        Entity = entity;
    }

    /// <summary>
    /// Checks if the component is alive before returning a mutable reference to the component data.
    /// It is not safe to access the <see cref="VRef{T}"/> if this method returns false.
    /// </summary>
    public bool TryGetValue(out VRef<T> value)
    {
        if (!IsAlive)
        {
            value = default;
            return false;
        }

        value = new VRef<T>(ref Value);
        return true;
    }

    public override string ToString()
    {
        return $"{Entity} ({TypeUtility.FormatConciseName<T>()})";
    }
}
