using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Exanite.Core.Pooling;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

public partial class EcsCommandBuffer
{
    private struct EntityState
    {
        public bool IsCreated;
        public bool IsDestroyed;
        public Dictionary<ComponentId, SetterId>? Sets;
        public OrderedListSet<ComponentId>? Removes;

        public Dictionary<ComponentId, SetterId> GetOrAcquireSets()
        {
            if (Sets == null)
            {
                Sets = SimplePool<Dictionary<ComponentId, SetterId>>.Acquire();
                Sets.Clear();
            }

            return Sets;
        }

        public OrderedListSet<ComponentId> GetOrAcquireRemoves()
        {
            if (Removes == null)
            {
                Removes = SimplePool<OrderedListSet<ComponentId>>.Acquire();
                Removes.Clear();
            }

            return Removes;
        }

        public void Release()
        {
            if (Sets != null)
            {
                SimplePool<Dictionary<ComponentId, SetterId>>.Release(Sets);
            }

            if (Removes != null)
            {
                SimplePool<OrderedListSet<ComponentId>>.Release(Removes);
            }
        }
    }

    private void ExecuteInternal()
    {
        using var _ = World.AcquireCommandBuffer(out var recursiveCommandBuffer);

        // TODO: Decide if I want to coalesce sets followed by removes
        // Same with creates followed by destroys
        // I probably do, since prefabs can have repeated sets and removes.

        // TODO: Add pooling
        var archetypesToDestroy = new List<Archetype>();
        var entityStates = new Dictionary<EntityId, EntityState>();
        var actions = new List<Action>();

        // Preprocess
        foreach (var command in state.Commands)
        {
            switch (command.Type)
            {
                case CommandType.CreateEntity:
                {
                    var typedCommand = state.CreateEntityCommands[command.Index];
                    ref var entityState = ref GetEntityState(typedCommand.EntityId);
                    entityState.IsCreated = true;

                    break;
                }
                case CommandType.DestroyEntity:
                {
                    var typedCommand = state.DestroyEntityCommands[command.Index];
                    ref var entityState = ref GetEntityState(typedCommand.EntityId);
                    entityState.IsDestroyed = true;

                    break;
                }
                case CommandType.DestroyArchetypeView:
                {
                    var typedCommand = state.DestroyArchetypeViewCommands[command.Index];
                    foreach (var archetype in typedCommand.View.Archetypes)
                    {
                        EnsureIsFromCurrentWorld(archetype);
                        archetypesToDestroy.Add(archetype);
                    }

                    break;
                }
                case CommandType.SetComponent:
                {
                    var typedCommand = state.SetComponentCommands[command.Index];
                    ref var entityState = ref GetEntityState(typedCommand.EntityId);

                    var sets = entityState.GetOrAcquireSets();
                    sets[typedCommand.SetterId.ComponentId] = typedCommand.SetterId;

                    break;
                }
                case CommandType.RemoveComponent:
                {
                    var typedCommand = state.RemoveComponentCommands[command.Index];
                    ref var entityState = ref GetEntityState(typedCommand.EntityId);

                    var removes = entityState.GetOrAcquireRemoves();
                    removes.Add(typedCommand.ComponentId);

                    break;
                }
                case CommandType.DeferredAction:
                {
                    var typedCommand = state.DeferredActionCommands[command.Index];
                    actions.Add(typedCommand.Action);

                    break;
                }
                default: throw ExceptionUtility.NotSupportedEnumValue(command.Type);
            }
        }

        // Apply changes
        // TODO

        // Release unused local IDs
        World.Entities.ReleaseUnusedIds(localIdPool);

        // Clear commands
        state.Clear(World);

        recursiveCommandBuffer.Execute();

        return;

        ref EntityState GetEntityState(EntityId entityId)
        {
            return ref CollectionsMarshal.GetValueRefOrAddDefault(entityStates, entityId, out var exists);
        }
    }
}
