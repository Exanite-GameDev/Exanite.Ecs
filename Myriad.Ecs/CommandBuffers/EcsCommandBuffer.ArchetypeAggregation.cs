// TODO: Not currently used, but likely will be in the future.
// using System;
// using System.Buffers;
// using System.Collections.Generic;
// using Exanite.Core.Pooling;
// using Exanite.Myriad.Ecs.Components;
// using Exanite.Myriad.Ecs.Worlds.Archetypes;
//
// namespace Exanite.Myriad.Ecs.CommandBuffers;
//
// public sealed partial class EcsCommandBuffer
// {
//     // Keep track of a fixed number of aggregation nodes. The root node (0) is the node for a new entity
//     // with no components. Nodes store a list of "edges" leading to other nodes. Edges indicate
//     // the addition of that component to the entity. Buffered entities keep track of their node ID. Every
//     // buffered entity with the same node ID therefore has the same archetype. Except for node=-1, which
//     // indicates unknown.
//
//     /// <summary>
//     /// Map from (currentArchetypeKey, addedComponent) => newArchetypeKey.
//     /// </summary>
//     private readonly Dictionary<(int, ComponentId), int> archetypeEdges = new();
//
//     private void CreateBufferedEntities(EcsCommandBuffer recursiveCommandBuffer)
//     {
//         // Keep a map from archetype key -> archetype.
//         // This means we only need to calculate it once per archetype key.
//         var archetypeLookup = ArrayPool<Archetype>.Shared.Rent(archetypeEdges.Count + 1);
//         Array.Clear(archetypeLookup, 0, archetypeLookup.Length);
//         try
//         {
//             foreach (var bufferedEntity in bufferedEntities)
//             {
//                 var components = bufferedEntity.Setters;
//                 try
//                 {
//                     var archetype = GetArchetype(bufferedEntity, archetypeLookup);
//                     var location = archetype.CreateEntity(recursiveCommandBuffer, bufferedEntity.Id);
//
//                     // Write the components into the entity
//                     foreach (var setter in components.Values)
//                     {
//                         // Write component
//                         setters.Write(setter, location);
//                     }
//
//                     // Raise component added events
//                     foreach (var setter in components.Values)
//                     {
//                         var eventDispatcher = archetype.Lookup.ComponentEventDispatcherByComponentId[setter.ComponentId.Value];
//                         eventDispatcher.OnComponentAdded(recursiveCommandBuffer, bufferedEntity.Id.ToEntity(World));
//                     }
//                 }
//                 finally
//                 {
//                     // Recycle
//                     components.Clear();
//                     SimplePool.Release(components);
//                 }
//             }
//
//             bufferedEntities.Clear();
//         }
//         finally
//         {
//             ArrayPool<Archetype>.Shared.Return(archetypeLookup);
//         }
//     }
//
//     private Archetype GetArchetype(BufferedEntityData entityData, Archetype?[] archetypeLookup)
//     {
//         // Check the cache
//         if (entityData.ArchetypeKey >= 0)
//         {
//             var a = archetypeLookup[entityData.ArchetypeKey];
//             if (a != null)
//             {
//                 return a;
//             }
//         }
//
//         // Get the archetype
//         var archetype = World.GetOrCreateArchetype(entityData.Setters.AsComponentIdSet());
//
//         // If the node ID is positive, cache it
//         if (entityData.ArchetypeKey >= 0)
//         {
//             archetypeLookup[entityData.ArchetypeKey] = archetype;
//         }
//
//         return archetype;
//     }
//
//     /// <summary>
//     /// Given an archetype key and an added component, determine the new archetype key.
//     /// </summary>
//     private int GetArchetypeKey(int currentKey, ComponentId addedComponent)
//     {
//         if (!archetypeEdges.TryGetValue((currentKey, addedComponent), out var newKey))
//         {
//             // Limit the number of edges to prevent explosive growth in some edge cases.
//             if (archetypeEdges.Count >= EcsConstants.MaxAggregationEdges)
//             {
//                 return -1;
//             }
//
//             newKey = archetypeEdges.Count + 1;
//             archetypeEdges.Add((currentKey, addedComponent), newKey);
//         }
//
//         return newKey;
//     }
// }
