using Myriad.Ecs.Worlds;
using System.Diagnostics.CodeAnalysis;

namespace Myriad.Ecs.Command;

/// <summary>
/// Provides a <see cref="CommandBuffer"/>, which is lazily created the first time it is accessed
/// </summary>
public struct LazyCommandBuffer
{
    private EcsCommandBuffer? _buffer;

    /// <summary>
    /// The <see cref="World"/> which this <see cref="LazyCommandBuffer"/> is for
    /// </summary>
    public World World { get; }

    /// <summary>
    /// Create a new <see cref="LazyCommandBuffer"/> for the given <see cref="World"/>
    /// </summary>
    /// <param name="world"></param>
    public LazyCommandBuffer(World world)
    {
        World = world;
        _buffer = null;
    }

    /// <summary>
    /// Get the buffer (constructing one if it does not yet exist)
    /// </summary>
    public EcsCommandBuffer CommandBuffer
    {
        get
        {
            _buffer ??= World.GetCommandBuffer();
            return _buffer;
        }
    }

    /// <summary>
    /// Get the buffer, or null if it does not yet exist
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public readonly bool TryGetBuffer([NotNullWhen(true)] out EcsCommandBuffer? buffer)
    {
        buffer = _buffer;
        return buffer != null;
    }
}
