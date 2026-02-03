namespace Exanite.Myriad.Ecs;

public interface IEntityLookup
{
    public EcsRef<T> Get<T>(EcsRef<T> reference, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist) where T : IComponent;

    public Entity Get(Entity entity, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist);
}
