namespace Exanite.Myriad.Ecs;

public interface IEntityLookup
{
    public Entity GetEntity(Entity entity, EntityLookupPolicy policy);
}
