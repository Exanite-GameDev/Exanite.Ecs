using System.Diagnostics.CodeAnalysis;

namespace Myriad.Ecs.CommandBuffers;

/// <summary>
/// Provides a <see cref="CommandBuffer"/>, which is lazily created the first time it is accessed
/// </summary>
public struct LazyCommandBuffer
{
    private EcsCommandBuffer? buffer;

    /// <summary>
    /// The <see cref="World"/> which this <see cref="LazyCommandBuffer"/> is for
    /// </summary>
    public World World { get; }

    /// <summary>
    /// Create a new <see cref="LazyCommandBuffer"/> for the given <see cref="World"/>
    /// </summary>
    public LazyCommandBuffer(World world)
    {
        World = world;
        buffer = null;
    }

    /// <summary>
    /// Get the buffer (constructing one if it does not yet exist)
    /// </summary>
    public EcsCommandBuffer CommandBuffer
    {
        get
        {
            buffer ??= World.GetCommandBuffer();
            return buffer;
        }
    }

    /// <summary>
    /// Get the buffer, or null if it does not yet exist
    /// </summary>
    public readonly bool TryGetBuffer([NotNullWhen(true)] out EcsCommandBuffer? buffer)
    {
        buffer = this.buffer;
        return buffer != null;
    }
}
