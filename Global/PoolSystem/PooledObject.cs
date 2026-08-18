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

    public virtual void Set(object config = null) { }

    protected virtual void OnReleaseToPool_raise() => OnReleaseToPool?.Invoke();
}
}