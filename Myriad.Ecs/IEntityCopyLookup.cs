namespace Exanite.Myriad.Ecs;

public interface IEntityCopyLookup
{
    public Entity GetEntity(Entity entity, EntityLookupPolicy policy);
}
