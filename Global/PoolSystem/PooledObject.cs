using System;
using UnityEngine;

namespace Helpers.PoolSystem
{
public class PooledObject : MonoBehaviour
{
    public event Action OnReleaseToPool;

    public virtual void Init(Action<PooledObject> onReleaseToPool, object config) => OnReleaseToPool += () => onReleaseToPool(this);

    public virtual void Set(object config = null) { }

    protected virtual void OnReleaseToPool_raise() => OnReleaseToPool?.Invoke();
}
}