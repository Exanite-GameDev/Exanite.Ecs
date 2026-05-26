using System;
using System.Collections.Generic;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// Stores information about the components stored in an archetype.
/// </summary>
internal record struct ArchetypeInfo
{
    /// <summary>
    /// The hash of all components IDs in this archetype.
    /// </summary>
    public readonly ArchetypeHash Hash;

    /// <summary>
    /// Sparse map from component ID to column index.
    /// </summary>
    public readonly int[] ColumnIndexByComponentId;

    /// <summary>
    /// Sparse map from component ID to component dispatcher.
    /// </summary>
    public readonly ComponentDispatcher[] ComponentDispatcherByComponentId;

    /// <summary>
    /// Sparse map from component ID to interface instance.
    /// </summary>
    public readonly object?[] InterfaceByInterfaceId;

    /// <summary>
    /// Map from column index to the component ID.
    /// </summary>
    public readonly ComponentId[] ComponentIdByColumnIndex;

    /// <summary>
    /// A bloom filter of all the components in this archetype.
    /// </summary>
    public readonly ComponentBloomFilter BloomFilter;

    /// <summary>
    /// The components of entities in this archetype, including interface components.
    /// </summary>
    public readonly IReadOnlyOrderedListSet<TypeId> Types;

    /// <summary>
    /// The components of entities in this archetype.
    /// </summary>
    public readonly IReadOnlyOrderedListSet<ComponentId> Components;

    /// <summary>
    /// The interface components of entities in this archetype.
    /// </summary>
    public readonly IReadOnlyOrderedListSet<InterfaceId> Interfaces;

    public ArchetypeInfo(IReadOnlyOrderedListSet<ComponentId> components)
    {
        Components = components.MakeSelfImmutable();

        // Interfaces are not available while bootstrapping the archetype info
        Interfaces = OrderedListSet<InterfaceId>.Empty;
        InterfaceByInterfaceId = [];

        // Build initial set of types
        {
            var types = new OrderedListSet<TypeId>();
            types.EnsureCapacity(components.Count);
            foreach (var componentId in components)
            {
                types.Add(componentId);
            }

            Types = types.MakeSelfImmutable();
        }

        // Create bloom filter
        BloomFilter = Types.Items.ToBloomFilter();

        // Calculate max component ID and archetype hash
        var maxComponentId = int.MinValue;
        foreach (var componentId in components)
        {
            Hash = Hash.Toggle(componentId);
            if (componentId.Value > maxComponentId)
            {
                maxComponentId = componentId.Value;
            }
        }

        // Initialize a map from column index to component ID
        ComponentIdByColumnIndex = new ComponentId[components.Count];

        // Initialize a sparse map from component ID to column index
        ColumnIndexByComponentId = maxComponentId == int.MinValue ? [] : new int[maxComponentId + 1];
        Array.Fill(ColumnIndexByComponentId, -1);

        // Fill previously mentioned maps
        for (var i = 0; i < components.Count; i++)
        {
            var componentId = components[i];
            ComponentIdByColumnIndex[i] = componentId;
            ColumnIndexByComponentId[componentId.Value] = i;
        }

        // Create a sparse map from component ID to component dispatcher
        ComponentDispatcherByComponentId = maxComponentId == int.MinValue ? [] : new ComponentDispatcher[maxComponentId + 1];
        foreach (var component in components)
        {
            ComponentDispatcherByComponentId[component.Value] = TypeRegistry.GetComponentDispatcher(component);
        }
    }

    public ArchetypeInfo(ArchetypeInfo existing, Dictionary<InterfaceId, object> interfaceComponents)
    {
        // Initialize with existing info
        this = existing;

        // Create interface type set
        Interfaces = OrderedListSet<InterfaceId>.Create(interfaceComponents).MakeSelfImmutable();

        // Build final set of types
        {
            var types = new OrderedListSet<TypeId>();
            types.EnsureCapacity(Components.Count + Interfaces.Count);

            foreach (var componentId in Components)
            {
                types.Add(componentId);
            }

            foreach (var interfaceId in Interfaces)
            {
                types.Add(interfaceId);
            }

            Types = types.MakeSelfImmutable();
        }

        // Create bloom filter
        BloomFilter = Types.Items.ToBloomFilter();

        // Calculate max interface index
        var maxInterfaceIndex = int.MinValue;
        foreach (var interfaceId in Interfaces)
        {
            var interfaceIndex = ~interfaceId.Value;
            if (interfaceIndex > maxInterfaceIndex)
            {
                maxInterfaceIndex = interfaceIndex;
            }
        }

        // Create a sparse map from interface ID to interface instance
        InterfaceByInterfaceId = maxInterfaceIndex == int.MinValue ? [] : new object[maxInterfaceIndex + 1];
        foreach (var (interfaceId, interfaceInstance) in interfaceComponents)
        {
            InterfaceByInterfaceId[~interfaceId.Value] = interfaceInstance;
        }
    }
}
