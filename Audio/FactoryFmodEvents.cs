using R3;
using System;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using Helpers.Audio;

namespace Helpers.PoolSystem
{
public class FactoryFmodEvents
{
    readonly TimeSpan _soundCheckinterval = TimeSpan.FromSeconds(.1);

    Pool<EventInstance> GetPool(EventReference eventReference, bool enable3DAttributes = false, int preloadCount = 10, int maxCount = 10)
    {
        Pool<EventInstance> pool = null;

        pool = new Pool<EventInstance>(OnCreateAction, OnGetAction, OnReleaseAction, OnDestroyAction, preloadCount, maxCount);
        return pool;

        EventInstance OnCreateAction(Action<EventInstance> returnToPoolAction)
        {
            var eventInstance = eventReference.GetInstance();
            if (enable3DAttributes)
                eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(Vector3.zero));

            return eventInstance;
        }

        void OnGetAction(EventInstance eventInstance)
        {
            Observable.Interval(_soundCheckinterval)
                      .TakeWhile(_ =>
                      {
                          eventInstance.getPlaybackState(out var state);
                          return state != PLAYBACK_STATE.STOPPED;
                      })
                      .Subscribe(_ => { }, _ => pool.Release(eventInstance));
        }

        void OnReleaseAction(EventInstance eventInstance) => eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        void OnDestroyAction(EventInstance eventInstance) => eventInstance.ReleaseInstance();
    }

    public class Builder
    {
        EventReference _eventReference;
        bool _enable3DAttributes;
        int _preloadCount;
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

            var pool = new FactoryFmodEvents().GetPool(_eventReference, _enable3DAttributes, _preloadCount, _maxCount);

            return pool;
        }
    }
}
}