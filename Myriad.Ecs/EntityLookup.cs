using System.Collections.Generic;
using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs;

public class EntityLookup : IEntityCopyLookup
{
    public readonly Dictionary<Entity, Entity> Lookup;

    public EntityLookup()
    {
        Lookup = new Dictionary<Entity, Entity>();
    }

    public EntityLookup(Dictionary<Entity, Entity> lookup)
    {
        Lookup = lookup;
    }

    public Entity GetEntity(Entity entity, EntityLookupPolicy policy)
    {
        if (Lookup.TryGetValue(entity, out var newEntity))
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
