using System;
using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace Helpers.Audio
{
public static class FmodExtensions
{
    static readonly Dictionary<string, List<EventInstance>> _cachedInstances = new();

#region Loading & Unloading Banks

    /// <summary>
    /// Loads an FMOD bank from Addressables, using FMOD RuntimeManager and waits until FMOD reports it as loaded
    /// </summary>
    public static async UniTask LoadBank(this AssetReference assetRef, bool loadSamples = false, int timeoutMs = 5000, CancellationToken cancelToken = default)
    {
        ValidateBankReference(assetRef);
        
        if (RuntimeManager.HasBankLoaded(assetRef.AssetGUID))
        {
            Debug.LogWarning($"[FmodExtensions]: FMOD bank '{assetRef.RuntimeKey}' is already loaded. Skipping load.");
            return;
        }

        try
        {
            var completionSource = new UniTaskCompletionSource();
            RuntimeManager.LoadBank(assetRef, loadSamples, () => completionSource.TrySetResult());

            await completionSource.Task
                                  .Timeout(TimeSpan.FromMilliseconds(timeoutMs))
                                  .AttachExternalCancellation(cancelToken);
        }
        catch (TimeoutException ex)
        {
            throw new TimeoutException($"[FmodExtensions]: Timed out while loading FMOD bank '{assetRef.RuntimeKey}'.", ex);
        }
    }

    /// <summary>
    /// Unloads an FMOD bank that was loaded via the same Addressable AssetReference.
    /// </summary>
    public static bool UnloadBank(this AssetReference assetRef)
    {
        if (assetRef == null || string.IsNullOrEmpty(assetRef.AssetGUID) || !RuntimeManager.IsInitialized)
            return false;

        if (!RuntimeManager.HasBankLoaded(assetRef.AssetGUID))
            return false;

        RuntimeManager.UnloadBank(assetRef);
        return true;
    }

    static void ValidateBankReference(AssetReference assetRef)
    {
        if (assetRef == null)
            throw new ArgumentNullException(nameof(assetRef));

        if (!assetRef.RuntimeKeyIsValid() || string.IsNullOrEmpty(assetRef.AssetGUID))
            throw new ArgumentException("[FmodExtensions]: AssetReference must have a valid runtime key and GUID.", nameof(assetRef));
    }

#endregion

#region EventInstances & Parameters controll

    /// <summary>
    /// Plays an FMOD event once at the given position.
    /// </summary>
    public static void PlayOneShot(this EventReference soundEvent, Vector3 pos = default) => RuntimeManager.PlayOneShot(soundEvent, pos);

    /// <summary>
    /// Creates an FMOD event instance and optionally caches it by scene.
    /// </summary>
    public static EventInstance GetInstance(this EventReference eventReference, string sceneName = "")
    {
        var eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstance.SaveInstanceByScene(sceneName);

        return eventInstance;
    }

    /// <summary>
    /// Creates an FMOD event instance and optionally caches it by scene.
    /// </summary>
    public static EventInstance GetInstance(this string eventReference, string sceneName = "")
    {
        var eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstance.SaveInstanceByScene(sceneName);

        return eventInstance;
    }

    /// <summary>
    /// Caches an FMOD event instance by scene name.
    /// </summary>
    public static void SaveInstanceByScene(this EventInstance eventInstance, string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        if (_cachedInstances.TryGetValue(sceneName, out var sceneEventInstances))
        {
            if (sceneEventInstances.Contains(eventInstance)) return;
            sceneEventInstances.Add(eventInstance);
        }
        else
        {
            sceneEventInstances = new();
            sceneEventInstances.Add(eventInstance);
            _cachedInstances.Add(sceneName, sceneEventInstances);
        }
    }

    /// <summary>
    /// Stops and releases all FMOD event instances cached for a scene.
    /// </summary>
    public static void ReleaseSceneInstances(string sceneName)
    {
        if (!_cachedInstances.TryGetValue(sceneName, out var sceneEventInstances)) return;

        for (int i = 0; i < sceneEventInstances.Count; i++)
            sceneEventInstances[i].ReleaseInstance();

        _cachedInstances.Remove(sceneName);
    }

    /// <summary>
    /// Stops and releases an FMOD event instance.
    /// </summary>
    public static void ReleaseInstance(this EventInstance eventInstance)
    {
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        eventInstance.release();
    }

    public static void SetParameter(this EventInstance eventInstance, string paramName, float paramValue) => eventInstance.setParameterByName(paramName, paramValue);

    public static void SetParameter(this EventInstance eventInstance, string paramName, string paramValue) => eventInstance.setParameterByNameWithLabel(paramName, paramValue);

#endregion
}
}