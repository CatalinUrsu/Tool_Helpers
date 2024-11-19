using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Helpers.PoolSystem
{
public class FactoryGO
{
    Pool<PooledObject> GetPool(PooledObject prefab, Transform poolActive, Transform poolInactive, object initConfig, int preloadCount, int maxCount)
    {
        return new Pool<PooledObject>(CreateAction, GetAction, ReleaseAction, DestroyAction, preloadCount, maxCount);

        PooledObject CreateAction(Action<PooledObject> returnToPoolAction)
        {
            var pooledObject = Object.Instantiate(prefab, poolInactive);
            pooledObject.Init(returnToPoolAction, initConfig);

            return pooledObject;
        }

        void ReleaseAction(PooledObject obj) => obj.transform.parent = poolInactive;

        void GetAction(PooledObject obj) => obj.transform.parent = poolActive;

        void DestroyAction(PooledObject obj) => Object.Destroy(obj.gameObject);
    }

    public class Builder
    {
        PooledObject _prefab;
        Transform _poolActive;
        Transform _poolInactive;
        object _config;
        int _preloadCount;
        int _maxCount = 10;

        public Builder(PooledObject prefab)
        {
            _prefab = prefab;
        }

        public Builder WithParents(Transform poolActive, Transform poolInactive)
        {
            _poolActive = poolActive;
            _poolInactive = poolInactive;
            return this;
        }

        public Builder WithItemInitConfig(object config)
        {
            _config = config;
            return this;
        }

        public Builder WithPreloadCount(int preloadCount)
        {
            _preloadCount = preloadCount;
            return this;
        }

        public Builder WithMaxCount(int maxCount)
        {
            _maxCount = maxCount;
            return this;
        }

        public Pool<PooledObject> Build()
        {
            return new FactoryGO().GetPool(_prefab, _poolActive, _poolInactive, _config, _preloadCount, _maxCount);
        }
    }
}
}