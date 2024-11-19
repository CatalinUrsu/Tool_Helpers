using UniRx;
using System;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;

namespace Helpers.PoolSystem
{
public class FactoryFmodEvents
{
    readonly TimeSpan _soundCheckinterval = TimeSpan.FromSeconds(.1);

    IPool<EventInstance> GetPool(EventReference eventReference, bool enable3DAttributes, int preloadCount, int maxCount)
    {
        IPool<EventInstance> pool = null;

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
                          return !state.Equals(PLAYBACK_STATE.STOPPED);
                      })
                      .DoOnCompleted(() => pool.Release(eventInstance))
                      .Subscribe();
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

        public Builder With3DAttributes(bool enable)
        {
            _enable3DAttributes = enable;
            return this;
        }

        public IPool<EventInstance> Build()
        {
            if (_maxCount < _preloadCount)
                _maxCount = _preloadCount;

            var pool = new FactoryFmodEvents().GetPool(_eventReference, _enable3DAttributes, _preloadCount, _maxCount);

            return pool;
        }
    }
}
}