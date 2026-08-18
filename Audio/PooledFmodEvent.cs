using System;
using FMODUnity;
using FMOD.Studio;
using System.Runtime.InteropServices;

namespace Helpers.Audio
{
public class PooledFmodEvent : IDisposable
{
    public EventInstance Instance => _instance;
    public event Action<PooledFmodEvent> OnReleaseToPool;

    GCHandle _handle;
    EventInstance _instance;

    public PooledFmodEvent(EventReference eventReference, Action<PooledFmodEvent> onStopped)
    {
        _instance = RuntimeManager.CreateInstance(eventReference);
        OnReleaseToPool = onStopped;

        _handle = GCHandle.Alloc(this);
        _instance.setUserData(GCHandle.ToIntPtr(_handle));
    }

    public void BindStoppedCallback() => _instance.setCallback(Callback, EVENT_CALLBACK_TYPE.STOPPED);

    public void UnbindStoppedCallback() => _instance.setCallback(null);

    public void Dispose()
    {
        UnbindStoppedCallback();
        _instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _instance.release();

        if (_handle.IsAllocated)
            _handle.Free();
    }

    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static FMOD.RESULT Callback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameters)
    {
        if (type != EVENT_CALLBACK_TYPE.STOPPED ||
            new EventInstance(instancePtr).getUserData(out var userData) != FMOD.RESULT.OK ||
            userData == IntPtr.Zero)
            return FMOD.RESULT.OK;

        if (GCHandle.FromIntPtr(userData).Target is PooledFmodEvent pooledEvent)
            pooledEvent.OnReleaseToPool?.Invoke(pooledEvent);

        return FMOD.RESULT.OK;
    }
}
}