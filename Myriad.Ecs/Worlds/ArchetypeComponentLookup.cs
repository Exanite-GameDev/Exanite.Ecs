using System;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// Stores component type mapping information for an archetype and its chunks.
/// </summary>
internal struct ArchetypeComponentLookup
{
    /// <summary>
    /// Map from column index to the component type.
    /// </summary>
    internal readonly Type[] ComponentTypesByColumnIndex;

    /// <summary>
    /// Map from column index to the component ID.
    /// </summary>
    internal readonly ComponentId[] ComponentIdByColumnIndex;

    /// <summary>
    /// Sparse map from component ID to column index.
    /// </summary>
    internal readonly int[] ColumnIndexByComponentId;

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

        // Initialize a map from column index to component type and component ID
        ComponentTypesByColumnIndex = new Type[components.Count];
        ComponentIdByColumnIndex = new ComponentId[components.Count];

        // Initialize a sparse map from component ID to column index
        ColumnIndexByComponentId = maxComponentId == int.MinValue ? [] : new int[maxComponentId + 1];
        Array.Fill(ColumnIndexByComponentId, -1);

        // Fill previously mentioned maps
        var columnIndex = 0;
        foreach (var component in components)
        {
            ComponentTypesByColumnIndex[columnIndex] = component.Type;
            ComponentIdByColumnIndex[columnIndex] = component;

            ColumnIndexByComponentId[component.Value] = columnIndex;

            columnIndex++;
        }

        // Create a sparse map from component ID to component event dispatcher
        ComponentEventDispatcherByComponentId = maxComponentId == int.MinValue ? [] : new ComponentEventDispatcher[maxComponentId + 1];
        foreach (var component in components)
        {
            ComponentEventDispatcherByComponentId[component.Value] = ComponentRegistry.GetComponentEventDispatcher(component);
        }
    }
}
