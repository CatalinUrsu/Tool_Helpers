using System;
using UnityEngine;

namespace Helpers.PoolSystem
{
public class PooledObject : MonoBehaviour
{
    public event Action OnReleaseToPool;

    public virtual PooledObject Init(Action<PooledObject> onReleaseToPool, object config = null)
    {
        OnReleaseToPool += () => onReleaseToPool(this);
        return this;
    }
    
    public virtual T Init<T>(Action<T> onReleaseToPool, object config = null) where T : PooledObject
    {
        var castedThis = (T)this;
        OnReleaseToPool += () => onReleaseToPool(castedThis);
        return castedThis;
    }

    public virtual void Set(object config = null) { }

    protected virtual void OnReleaseToPool_raise() => OnReleaseToPool?.Invoke();
}
}