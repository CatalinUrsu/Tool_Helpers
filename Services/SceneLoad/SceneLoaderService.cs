using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Helpers.Services
{
public class SceneLoaderService : ISceneLoaderService
{
#region Fieds

    readonly Dictionary<string, SceneInstance> _loadedAddressableScene = new();
    readonly IProgressTrackingService _progressTrackingService;

#endregion

#region Public methods

    public SceneLoaderService(IProgressTrackingService progressTrackingService) => _progressTrackingService = progressTrackingService;

    public async UniTask LoadScene(SceneLoadParams sceneLoadParams, SceneLoadResult loadResult)
    {
        if (!string.IsNullOrEmpty(sceneLoadParams.LoadingTip))
            _progressTrackingService.UpdateLoadingTip(sceneLoadParams.LoadingTip);

        await (sceneLoadParams.IsAddressable ? LoadAddressableScene(sceneLoadParams, loadResult) : LoadBuildScene(sceneLoadParams, loadResult));
    }

    public async UniTask UnloadScene(string sceneName)
    {
        try
        {
            await (_loadedAddressableScene.ContainsKey(sceneName) ? UnloadByAddressable(sceneName) : UnloadBySceneManager(sceneName));
        }
        catch (Exception e)
        {
            Debug.LogError($"Something goes wrong during scene unloading: {e.Message}.");
        }
    }

#endregion

#region Private methods

    async UniTask LoadAddressableScene(SceneLoadParams sceneLoadParams, SceneLoadResult loadingSceneData)
    {
        if (_loadedAddressableScene.TryGetValue(sceneLoadParams.SceneName, out var value))
        {
            loadingSceneData.SceneLoadProgress.LoadProgress = 1;
            loadingSceneData.LoadedScene = value.Scene;
        }
        else
        {
            var progress = SubscribeProgressToValueChange(loadingSceneData);
            var asyncOperationHandle = Addressables.LoadSceneAsync(sceneLoadParams.SceneName, sceneLoadParams.LoadParameters.loadSceneMode);

            await asyncOperationHandle.ToUniTask(progress);

            loadingSceneData.SceneLoadProgress.LoadProgress = 1;
            loadingSceneData.LoadedScene = asyncOperationHandle.Result.Scene;
            OnLoadingSceneComplete_handler(sceneLoadParams.SceneName, loadingSceneData.LoadedScene, sceneLoadParams.SetSceneActive);
        }

        await UniTask.CompletedTask;
    }

    async UniTask LoadBuildScene(SceneLoadParams sceneLoadParams, SceneLoadResult loadingSceneData)
    {
        if (SceneManager.GetSceneByName(sceneLoadParams.SceneName).isLoaded)
        {
            loadingSceneData.SceneLoadProgress.LoadProgress = 1;
            loadingSceneData.LoadedScene = SceneManager.GetSceneByName(sceneLoadParams.SceneName);
        }
        else
        {
            var progress = SubscribeProgressToValueChange(loadingSceneData);
            var asyncOperation = SceneManager.LoadSceneAsync(sceneLoadParams.SceneName, sceneLoadParams.LoadParameters);

            await asyncOperation.ToUniTask(progress);

            loadingSceneData.SceneLoadProgress.LoadProgress = 1;
            loadingSceneData.LoadedScene = SceneManager.GetSceneByName(sceneLoadParams.SceneName);
            OnLoadingSceneComplete_handler(sceneLoadParams.SceneName, loadingSceneData.LoadedScene, sceneLoadParams.SetSceneActive);
        }

        await UniTask.CompletedTask;
    }

    static IProgress<float> SubscribeProgressToValueChange(SceneLoadResult sceneLoadResult) => 
        Progress.CreateOnlyValueChanged<float>(x => sceneLoadResult.SceneLoadProgress.LoadProgress = x);

    UniTask UnloadBySceneManager(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.isLoaded)
        {
            Debug.LogError($"There's no active scene with name '{sceneName}'.");
            return new UniTask();
        }

        return SceneManager.UnloadSceneAsync(scene).ToUniTask();
    }

    UniTask UnloadByAddressable(string sceneName)
    {
        if (!_loadedAddressableScene.ContainsKey(sceneName))
        {
            Debug.Log($"[ServiceSceneLoader]: UnloadAddressableScene - There's no scene {sceneName} in dictionary");
            return UniTask.NextFrame();
        }

        var asyncOperationHandle = Addressables.UnloadSceneAsync(_loadedAddressableScene[sceneName]);
        asyncOperationHandle.Completed += _ => _loadedAddressableScene.Remove(sceneName);
        return asyncOperationHandle.ToUniTask();
    }

    void OnLoadingSceneComplete_handler<T>(string sceneName, T result, bool setActiveScene)
    {
        Scene scene = default;
        switch (result)
        {
            case SceneInstance instance:
                _loadedAddressableScene.Add(sceneName, instance);
                scene = instance.Scene;
                break;
            case Scene sceneResult:
                scene = sceneResult;
                break;
        }

        if (setActiveScene)
            SceneManager.SetActiveScene(scene);
    }

#endregion
}
}