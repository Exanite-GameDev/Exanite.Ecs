using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Exanite.Core.Pooling;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

public partial class EcsCommandBuffer
{
    private readonly struct CommandState
    {
        public readonly List<Command> Commands = [];

        public readonly List<CreateEntityCommand> CreateEntityCommands = [];
        public readonly List<DestroyEntityCommand> DestroyEntityCommands = [];
        public readonly List<DestroyArchetypeViewCommand> DestroyArchetypeViewCommands = [];
        public readonly List<SetComponentCommand> SetComponentCommands = [];
        public readonly List<RemoveComponentCommand> RemoveComponentCommands = [];
        public readonly List<DeferredActionCommand> DeferredActionCommands = [];

        public readonly ComponentSetterCollection Setters = new();

        public CommandState() {}

        public void Clear(EcsWorld world, bool hasExecuted)
        {
            // Release used entity IDs
            // Do not reuse these without releasing since external callers already have access to them
            foreach (var command in CreateEntityCommands)
            {
                world.Entities.ReleaseId(command.EntityId);
            }

            Commands.Clear();

            CreateEntityCommands.Clear();
            DestroyEntityCommands.Clear();
            DestroyArchetypeViewCommands.Clear();
            SetComponentCommands.Clear();
            RemoveComponentCommands.Clear();
            DeferredActionCommands.Clear();

            Setters.Clear(hasExecuted);
        }
    }

    private enum CommandType
    {
        CreateEntity,
        DestroyEntity,
        DestroyArchetypeView,
        SetComponent,
        RemoveComponent,
        DeferredAction,
    }

    private struct Command
    {
        public required CommandType Type;
        public required int Index;

        [SetsRequiredMembers]
        public Command(CommandType type, int index)
        {
            Type = type;
            Index = index;
        }
    }

    private struct CreateEntityCommand
    {
        public required EntityId EntityId;

        [SetsRequiredMembers]
        public CreateEntityCommand(EntityId entityId)
        {
            EntityId = entityId;
        }
    }

    private struct DestroyEntityCommand
    {
        public required EntityId EntityId;

        [SetsRequiredMembers]
        public DestroyEntityCommand(EntityId entityId)
        {
            EntityId = entityId;
        }
    }

    private struct DestroyArchetypeViewCommand
    {
        public required IArchetypeView View;

        [SetsRequiredMembers]
        public DestroyArchetypeViewCommand(IArchetypeView view)
        {
            View = view;
        }
    }

    private struct SetComponentCommand
    {
        public required EntityId EntityId;
        public required SetterId SetterId;

        [SetsRequiredMembers]
        public SetComponentCommand(EntityId entityId, SetterId setterId)
        {
            EntityId = entityId;
            SetterId = setterId;
        }
    }

    private struct RemoveComponentCommand
    {
        public required EntityId EntityId;
        public required ComponentId ComponentId;

        [SetsRequiredMembers]
        public RemoveComponentCommand(EntityId entityId, ComponentId componentId)
        {
            EntityId = entityId;
            ComponentId = componentId;
        }
    }

    private struct DeferredActionCommand
    {
        public required Action Action;

        [SetsRequiredMembers]
        public DeferredActionCommand(Action action)
        {
            Action = action;
        }
    }

    private readonly struct SetterId
    {
        /// <summary>
        /// Component ID of the component being overwritten.
        /// </summary>
        internal readonly ComponentId ComponentId;

        /// <summary>
        /// Index of the value in the values list.
        /// </summary>
        internal readonly int Index;

        internal SetterId(ComponentId componentId, int index)
        {
            ComponentId = componentId;
            Index = index;
        }
    }

    /// <summary>
    /// Contains values of components to be set onto entities.
    /// Involves no boxing.
    /// </summary>
    private class ComponentSetterCollection
    {
        private readonly Dictionary<ComponentId, IComponentList> components = [];

        /// <summary>
        /// Add a new component value to the collection.
        /// </summary>
        public SetterId Create<T>(T value) where T : IComponent
        {
            var id = ComponentId.Get<T>();

            if (!components.TryGetValue(id, out var list))
            {
                list = SimplePool<ComponentList<T>>.Acquire();
                components.Add(id, list);
            }

            var index = ((ComponentList<T>)list).Add(value);
            return new SetterId(id, index);
        }

        /// <summary>
        /// Output the stored component value to the specified entity location.
        /// </summary>
        public void Write(SetterId id, EntityLocation location)
        {
            var list = components[id.ComponentId];
            list.Write(id.Index, location);
        }

        /// <summary>
        /// Clear all component values stored in this collection.
        /// </summary>
        public void Clear(bool hasExecuted)
        {
            foreach (var (_, value) in components)
            {
                value.Recycle(hasExecuted);
            }

            components.Clear();
        }

        private interface IComponentList
        {
            public void Write(int index, EntityLocation location);
            public void Recycle(bool hasExecuted);
        }

        private class ComponentList<T> : IComponentList where T : IComponent
        {
            private readonly List<T> values = [];

            public int Add(T value)
            {
                values.Add(value);
                return values.Count - 1;
            }

            public void Recycle(bool hasExecuted)
            {
                if (!hasExecuted && values.Count > 0 && values[0] is IDisposable)
                {
                    foreach (var component in values)
                    {
                        ((IDisposable)component).Dispose();
                    }
                }

                values.Clear();
                SimplePool.Release(this);
            }

            public void Write(int index, EntityLocation location)
            {
                location.Chunk.Get<T>(location.IndexInChunk) = values[index];
            }
        }
    }
}
