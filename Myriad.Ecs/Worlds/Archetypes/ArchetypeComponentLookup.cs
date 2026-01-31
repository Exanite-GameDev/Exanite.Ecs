using System;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Worlds.Archetypes;

/// <summary>
/// Stores component type mapping information for an archetype and its chunks.
/// </summary>
internal struct ArchetypeComponentLookup
{
    /// <summary>
    /// Map from component index to the component type.
    /// </summary>
    internal readonly Type[] ComponentTypesByComponentIndex;

    /// <summary>
    /// Map from component index to the component ID.
    /// </summary>
    internal readonly ComponentId[] ComponentIdByComponentIndex;

    /// <summary>
    /// Sparse map from component ID to component index.
    /// </summary>
    internal readonly int[] ComponentIndexByComponentId;

    /// <summary>
    /// Sparse map from component ID to component event dispatcher.
    /// </summary>
    internal readonly ComponentEventDispatcher[] ComponentEventDispatcherByComponentId;

    public ArchetypeComponentLookup(ImmutableOrderedListSet<ComponentId> components)
    {
        // Calculate max component ID
        var maxComponentId = int.MinValue;
        foreach (var component in components)
        {
            if (component.Value > maxComponentId)
            {
                maxComponentId = component.Value;
            }
        }

        // Initialize a map from component index to component type and component ID
        ComponentTypesByComponentIndex = new Type[components.Count];
        ComponentIdByComponentIndex = new ComponentId[components.Count];

        // Initialize a sparse map from component ID to component index
        ComponentIndexByComponentId = maxComponentId == int.MinValue ? [] : new int[maxComponentId + 1];
        Array.Fill(ComponentIndexByComponentId, -1);

        // Fill previously mentioned maps
        var componentIndex = 0;
        foreach (var component in components)
        {
            ComponentTypesByComponentIndex[componentIndex] = component.Type;
            ComponentIdByComponentIndex[componentIndex] = component;

            ComponentIndexByComponentId[component.Value] = componentIndex;

            componentIndex++;
        }

        // Create a sparse map from component ID to component event dispatcher
        ComponentEventDispatcherByComponentId = maxComponentId == int.MinValue ? [] : new ComponentEventDispatcher[maxComponentId + 1];
        foreach (var component in components)
        {
            ComponentEventDispatcherByComponentId[component.Value] = ComponentRegistry.GetComponentEventDispatcher(component);
        }
    }
}
