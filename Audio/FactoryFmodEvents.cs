using System;
using FMODUnity;
using UnityEngine;
using Helpers.PoolSystem;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Helpers.Audio
{
/// <summary>
/// Creates pooled FMOD event instances and returns them to the pool after playback stops.
/// </summary>
public class FactoryFmodEvents
{
#region Fields

    EventReference _eventReference;
    bool _enable3DAttributes;
    int _preloadCount;
    int _maxCount;

#endregion

#region Methods

    Pool<PooledFmodEvent> GetPool() => new(OnCreateAction, OnGetAction, OnReleaseAction, OnDestroyAction, _preloadCount, _maxCount);
    
    PooledFmodEvent OnCreateAction(Action<PooledFmodEvent> returnToPoolAction)
    {
        var pooledEvent = new PooledFmodEvent(_eventReference, returnToPoolAction);
        var eventInstance = pooledEvent.Instance;

        if (_enable3DAttributes)
            eventInstance.set3DAttributes(Vector3.zero.To3DAttributes());

        return pooledEvent;
    }

    void OnGetAction(PooledFmodEvent pooledEvent) => pooledEvent.BindStoppedCallback();

    void OnReleaseAction(PooledFmodEvent pooledEvent)
    {
        // Prevent callback re-entry while pool-initiated release is running.
        pooledEvent.UnbindStoppedCallback();
        pooledEvent.Instance.stop(STOP_MODE.IMMEDIATE);
    }

    void OnDestroyAction(PooledFmodEvent pooledEvent) => pooledEvent.Dispose();

#endregion

#region Builder

    public class Builder
    {
        EventReference _eventReference;
        bool _enable3DAttributes;
        int _preloadCount = 10;
        int _maxCount = 10;

        public Builder(EventReference eventReference)
        {
            _eventReference = eventReference;
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

        public Builder Set3DAttributes(bool enable)
        {
            _enable3DAttributes = enable;
            return this;
        }

        public Pool<PooledFmodEvent> Build()
        {
            if (_maxCount < _preloadCount)
                _maxCount = _preloadCount;

            return new FactoryFmodEvents
            {
                _eventReference = _eventReference,
                _enable3DAttributes = _enable3DAttributes,
                _preloadCount = _preloadCount,
                _maxCount = _maxCount,
            }.GetPool();
        }
    }

#endregion
}
}