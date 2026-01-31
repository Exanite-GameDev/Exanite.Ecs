using System.Collections.Generic;
using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs;

public class EntityLookup
{
    public readonly IReadOnlyDictionary<Entity, Entity> Entries;

    public EntityLookup(Dictionary<Entity, Entity> entries)
    {
        Entries = entries;
    }

    public EcsRef<T> GetRef<T>(EcsRef<T> reference, EntityLookupPolicy policy) where T : IComponent
    {
        return new EcsRef<T>(GetEntity(reference.Entity, policy));
    }

    public Entity GetEntity(Entity entity, EntityLookupPolicy policy)
    {
        if (Entries.TryGetValue(entity, out var newEntity))
        {
            return newEntity;
        }

        return policy switch
        {
            EntityLookupPolicy.PreserveIfNotExist => entity,
            EntityLookupPolicy.DefaultIfNotExist => default,
            _ => throw ExceptionUtility.NotSupportedEnumValue(policy),
        };
    }
}
