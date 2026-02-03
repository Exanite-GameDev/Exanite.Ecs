using System;
using System.Collections.Generic;
using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs;

public class EntityLookup : IEntityLookup
{
    public readonly Dictionary<Entity, Entity> Entries;

    public EntityLookup()
    {
        Entries = [];
    }

    public EntityLookup(Dictionary<Entity, Entity> entries)
    {
        Entries = entries;
    }

    public void Add(Entity from, Entity to)
    {
        Entries.Add(from, to);
    }

    public EcsRef<T> Get<T>(EcsRef<T> reference, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist) where T : IComponent
    {
        return new EcsRef<T>(Get(reference.Entity, policy));
    }

    public Entity Get(Entity entity, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist)
    {
        if (Entries.TryGetValue(entity, out var newEntity))
        {
            return newEntity;
        }

        return policy switch
        {
            EntityLookupPolicy.PreserveIfNotExist => entity,
            EntityLookupPolicy.DefaultIfNotExist => default,
            EntityLookupPolicy.ThrowIfNotExist => throw new InvalidOperationException("Entity does not exist in this lookup table"),
            _ => throw ExceptionUtility.NotSupportedEnumValue(policy),
        };
    }
}
