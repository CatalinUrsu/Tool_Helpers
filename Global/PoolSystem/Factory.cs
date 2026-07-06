using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Helpers.PoolSystem
{
public class Factory
{
#region Fields

    PooledObject _prefab;
    object _config;
    Transform _activeParent;
    Transform _inactiveParent;
    int _preloadCount;
    int _maxCount;

#endregion

#region Methods

    Pool<PooledObject> GetPool() => new(Create, OnGet, OnRelease, OnDestroy, _preloadCount, _maxCount);

    PooledObject Create(Action<PooledObject> returnToPool)
    {
        return Object.Instantiate(_prefab, _inactiveParent)
                     .Init(returnToPool, _config);
    }

    void OnGet(PooledObject obj) => obj.transform.SetParent(_activeParent);

    void OnRelease(PooledObject obj) => obj.transform.SetParent(_inactiveParent);

    void OnDestroy(PooledObject obj) => Object.Destroy(obj.gameObject);

#endregion

#region Builder

    public class Builder
    {
        readonly PooledObject _prefab;
        object _config;
        Transform _activeParent;
        Transform _inactiveParent;
        int _preloadCount = 10;
        int _maxCount = 10;

        public Builder(PooledObject prefab) => _prefab = prefab;

        public Builder SetConfig(object config)
        {
            _config = config;
            return this;
        }

        public Builder SetParents(Transform activeParent, Transform inactiveParent)
        {
            _activeParent = activeParent;
            _inactiveParent = inactiveParent;
            return this;
        }

        public Builder SetPreloadCount(int preloadCount)
        {
            _preloadCount = preloadCount;
            return this;
        }

        public Builder SetMaxCount(int maxCount)
        {
            _maxCount = maxCount;
            return this;
        }

        public virtual Pool<PooledObject> Build()
        {
            return new Factory
            {
                _prefab = _prefab,
                _config = _config,
                _activeParent = _activeParent,
                _inactiveParent = _inactiveParent,
                _preloadCount = _preloadCount,
                _maxCount = _maxCount
            }.GetPool();
        }
    }

#endregion
}
}