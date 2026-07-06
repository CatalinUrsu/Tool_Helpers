using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
#endif

namespace Helpers.Audio
{
public static class FmodExtensions
{
    static readonly Dictionary<string, List<EventInstance>> _cachedInstances = new();

#region Loading & Unloading Banks

    /// <summary>
    /// Loads a text asset as an FMOD bank using the provided asset reference.
    /// </summary>
    public static async UniTask<BankData> LoadBank(this AssetReference assetRef)
    {
        var textAsset = await Addressables.LoadAssetAsync<TextAsset>(assetRef);
     var bank = new Bank();

#if UNITY_EDITOR
        if (AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex == 0)
            return new BankData();
#endif

     var result = RuntimeManager.StudioSystem.loadBankMemory(textAsset.bytes, LOAD_BANK_FLAGS.NORMAL, out bank);
     if (result == FMOD.RESULT.OK)
         Debug.Log("FMOD Bank loaded successfully.");
     else
         Debug.LogError("Failed to load FMOD Bank: " + result);

     return new BankData(bank, textAsset);
    }

    /// <summary>
    /// Unloads an FMOD bank and releases the associated text asset.
    /// </summary>
    public static void UnloadBank(this BankData bankData)
    {
#if UNITY_EDITOR
        if (AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex == 0)
            return;
#endif
        
        bankData.Bank.unload();
        Addressables.Release(bankData.TextAsset);
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