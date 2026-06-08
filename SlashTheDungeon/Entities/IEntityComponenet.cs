using UnityEngine;

public interface IEntityComponent
{
    public void Initialize(Entity entity);
}

public interface IentityComponent<T> : IEntityComponent
{
    public void Initialize(T entity);
}