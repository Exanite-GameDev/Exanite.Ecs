using System.Collections.Generic;

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

        return EntityLookupUtility.HandleLookupPolicy(entity, policy);
    }
}
