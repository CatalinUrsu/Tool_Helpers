using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
#endif

namespace Helpers
{
public static class FmodExtensions
{
    static readonly Dictionary<string, List<EventInstance>> _cachedInstances = new();

#region Loading & Unloading Banks

    public static async UniTask<TextAsset> LoadTextAsset(this AssetReference masterAssetRef) => await Addressables.LoadAssetAsync<TextAsset>(masterAssetRef);

    public static Bank LoadBank(this TextAsset bankTextAsset)
    {
        Bank bank = new Bank();

#if UNITY_EDITOR
        if (AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex == 0)
            return bank;
#endif

        var result = RuntimeManager.StudioSystem.loadBankMemory(bankTextAsset.bytes, LOAD_BANK_FLAGS.NORMAL, out bank);
        if (result == FMOD.RESULT.OK)
            Debug.Log("FMOD Bank loaded successfully.");
        else
            Debug.LogError("Failed to load FMOD Bank: " + result);

        return bank;
    }
    
    public static void UnloadBank(this Bank bank, AsyncOperationHandle<TextAsset> operationHandle)
    {
#if UNITY_EDITOR
        if (AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex == 0)
            return;
#endif
        
        bank.unload();
        Addressables.Release(operationHandle);
    }

    public static void UnloadBank(this Bank bank, TextAsset objectToRelease)
    {
#if UNITY_EDITOR
        if (AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex == 0)
            return;
#endif

        bank.unload();
        Addressables.Release(objectToRelease);
    }

#endregion

#region EventInstances & Parameters controll

    public static void PlayOneShot(this EventReference soundEvent, Vector3 pos = default) => RuntimeManager.PlayOneShot(soundEvent, pos);

    public static EventInstance GetInstance(this EventReference eventReference, string sceneName = "")
    {
        var eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstance.SaveInstanceByScene(sceneName);

        return eventInstance;
    }

    public static EventInstance GetInstance(this string eventReference, string sceneName = "")
    {
        var eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstance.SaveInstanceByScene(sceneName);

        return eventInstance;
    }

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

    public static void ReleaseInstanceByScene(string sceneName)
    {
        if (!_cachedInstances.TryGetValue(sceneName, out var sceneEventInstances)) return;

        for (int i = 0; i < sceneEventInstances.Count; i++)
            sceneEventInstances[i].ReleaseInstance();

        _cachedInstances.Remove(sceneName);
    }

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