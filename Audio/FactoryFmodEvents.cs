using FMOD;
using System;
using FMODUnity;
using FMOD.Studio;
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
    event Action<EventInstance> OnInstanceStop;

#endregion

#region Methods

    Pool<EventInstance> GetPool()
    {
        var pool  = new Pool<EventInstance>(OnCreateAction, OnGetAction, OnReleaseAction, OnDestroyAction, _preloadCount, _maxCount);
        OnInstanceStop += pool.Release;
        return pool;
    }
    
    EventInstance OnCreateAction(Action<EventInstance> returnToPoolAction)
    {
        var eventInstance = _eventReference.GetInstance();
        if (_enable3DAttributes)
            eventInstance.set3DAttributes(Vector3.zero.To3DAttributes());

        return eventInstance;
    }

    void OnGetAction(EventInstance eventInstance)
    {
        eventInstance.setCallback((_, _, _) => OnInstanceStopped(), EVENT_CALLBACK_TYPE.STOPPED);
        return;

        RESULT OnInstanceStopped()
        {
            OnInstanceStop?.Invoke(eventInstance);
            return RESULT.OK;
        }
    }

    void OnReleaseAction(EventInstance eventInstance) => eventInstance.stop(STOP_MODE.IMMEDIATE);

    void OnDestroyAction(EventInstance eventInstance) => eventInstance.ReleaseInstance();

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

        public Pool<EventInstance> Build()
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