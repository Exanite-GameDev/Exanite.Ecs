using System;
using System.Collections.Generic;
using Exanite.Core.Runtime;
using Exanite.Core.Threading;
using Exanite.Core.Utilities;

namespace Exanite.Ecs.Components;

/// <summary>
/// Stores information about component IDs.
/// </summary>
internal static partial class TypeRegistry
{
    private static readonly ReadWriteLock<State> Lock = new(new State());

    private class State
    {
        private readonly List<Type> typesByComponentId = [];
        private readonly List<ComponentDispatcher> dispatchersByComponentId = [];

        private readonly List<Type> typesByInterfaceId = [];

        private readonly Dictionary<Type, TypeId> typeIdByType = [];

        // 0 represents an invalid ID, so 1 is the first valid ID
        // Similarly for interface ids, which decrement instead
        private int nextComponentId = 1;
        private int nextInterfaceId = -1;

        public TypeId GetOrAddTypeId(Type type)
        {
            if (type.IsValueType)
            {
                return GetOrAddComponentId(type);
            }

            if (type.IsInterface)
            {
                return GetOrAddInterfaceId(type);
            }

            throw new GuardException("Components must be either structs or interfaces");
        }

        public ComponentId GetOrAddComponentId(Type type)
        {
            if (!typeIdByType.TryGetValue(type, out var typeId))
            {
                GuardUtility.IsTrue(type.IsValueType, "Storage-backed components must be structs");
                GuardUtility.IsTrue(type.IsConcrete(), "Storage-backed components must be concrete types");
                GuardUtility.IsTrue(typeof(IEcsComponent).IsAssignableFrom(type), $"{type.FullName} must implement the {nameof(IEcsComponent)} interface");

                // Get next ID
                typeId = new TypeId(nextComponentId);
                nextComponentId++;

                // Store for lookups
                typesByComponentId.Add(type);
                typeIdByType[type] = typeId;

                // Initialize the component dispatcher for this type
                var dispatcherType = typeof(ComponentDispatcher<>).MakeGenericType(type);
                var untypedDispatcher = Activator.CreateInstance(dispatcherType);
                if (untypedDispatcher is not ComponentDispatcher dispatcher)
                {
                    throw new GuardException($"Failed to create component dispatcher for type: {type}");
                }

                dispatchersByComponentId.Add(dispatcher);

                // Raise id registered event
                TypeId.NotifyIdRegistered(typeId);
            }

            return (ComponentId)typeId;
        }

        public InterfaceId GetOrAddInterfaceId(Type type)
        {
            if (!typeIdByType.TryGetValue(type, out var typeId))
            {
                GuardUtility.IsTrue(type.IsInterface, "Interface components must be interfaces");
                GuardUtility.IsTrue(typeof(IEcsInterface).IsAssignableFrom(type), $"{type.FullName} must implement the {nameof(IEcsInterface)} interface");

                // Get next ID
                typeId = new TypeId(nextInterfaceId);
                nextInterfaceId--;

                // Store for lookups
                typesByInterfaceId.Add(type);
                typeIdByType[type] = typeId;

                // Raise id registered event
                TypeId.NotifyIdRegistered(typeId);
            }

            return (InterfaceId)typeId;
        }

        public bool TryGetTypeId(Type type, out TypeId typeId)
        {
            return typeIdByType.TryGetValue(type, out typeId);
        }

        public Type GetBackingType(TypeId typeId)
        {
            if (typeId.IsComponent)
            {
                return TypeRegistry.GetComponentType((ComponentId)typeId);
            }

            return TypeRegistry.GetInterfaceType((InterfaceId)typeId);
        }

        public Type GetComponentType(ComponentId componentId)
        {
            return typesByComponentId[componentId.Value - 1];
        }

        public Type GetInterfaceType(InterfaceId interfaceId)
        {
            return typesByInterfaceId[~interfaceId.Value];
        }

        public ComponentDispatcher GetComponentDispatcher(ComponentId componentId)
        {
            return dispatchersByComponentId[componentId.Value - 1];
        }
    }
}
