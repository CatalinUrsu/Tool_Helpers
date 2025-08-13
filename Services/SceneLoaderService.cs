using System;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Helpers.Services
{
public class SceneLoaderService : IServiceSceneLoader
{
#region Fieds

    readonly Dictionary<string, SceneInstance> _loadedAddressableScene = new();
    readonly IServiceProgressTracking _serviceProgressTracking;

#endregion

#region Public methods

    public SceneLoaderService(IServiceProgressTracking serviceProgressTracking) => _serviceProgressTracking = serviceProgressTracking;
    

    public async UniTask<SceneLoadResult[]> LoadScenes(params SceneLoadParams[] sceneLoadParams)
    {
        _serviceProgressTracking.LoadProgressCount = sceneLoadParams.Count(loadParams => loadParams.TrackProgress);

        var scenesLoadTasks = sceneLoadParams.Select(GetSceneLoadTask).ToArray();
        return await UniTask.WhenAll(scenesLoadTasks);

        async UniTask<SceneLoadResult> GetSceneLoadTask(SceneLoadParams loadParams)
            {
                var sceneLoadResult = await GetSceneLoadResult(loadParams);

                if (!string.IsNullOrEmpty(loadParams.LoadingTip))
                    _serviceProgressTracking.UpdateLoadingTip(loadParams.LoadingTip);

                return sceneLoadResult;
            }
    }

    public async UniTask UnloadScene(string sceneName)
    {
        try
        {
            await (_loadedAddressableScene.ContainsKey(sceneName)
                ? UnloadByAddressable(sceneName)
                : UnloadBySceneManager(sceneName));
        }
        catch (Exception e)
        {
            Debug.LogError($"Something goes wrong during scene unloading: {e.Message}.");
        }
    }

#endregion

#region Private methods

    UniTask<SceneLoadResult> GetSceneLoadResult(SceneLoadParams sceneLoadParams)
    {
        return sceneLoadParams.IsAddressable
            ? LoadAddressableScene(sceneLoadParams)
            : LoadBuildScene(sceneLoadParams);
    }

    async UniTask<SceneLoadResult> LoadAddressableScene(SceneLoadParams sceneLoadParams)
    {
        var loadingSceneData = new SceneLoadResult();

        if (_loadedAddressableScene.TryGetValue(sceneLoadParams.SceneName, out var value))
        {
            loadingSceneData.LoadedScene = value.Scene;
        }
        else
        {
            var progress = sceneLoadParams.TrackProgress ? GetNewLoadingProgress(loadingSceneData) : new Progress<float>();
            var asyncOperationHandle = Addressables.LoadSceneAsync(sceneLoadParams.SceneName, sceneLoadParams.LoadParameters.loadSceneMode);

            await asyncOperationHandle.ToUniTask(progress);

            loadingSceneData.LoadedScene = asyncOperationHandle.Result.Scene;
            OnLoadingSceneComplete_handler(sceneLoadParams.SceneName, loadingSceneData.LoadedScene, sceneLoadParams.SetSceneActive);
        }

        return loadingSceneData;
    }

    async UniTask<SceneLoadResult> LoadBuildScene(SceneLoadParams sceneLoadParams)
    {
        var loadingSceneData = new SceneLoadResult();

        if (SceneManager.GetSceneByName(sceneLoadParams.SceneName).isLoaded)
        {
            loadingSceneData.LoadedScene = SceneManager.GetSceneByName(sceneLoadParams.SceneName);
        }
        else
        {
            var progress = sceneLoadParams.TrackProgress ? GetNewLoadingProgress(loadingSceneData) : new Progress<float>();
            var asyncOperation = SceneManager.LoadSceneAsync(sceneLoadParams.SceneName, sceneLoadParams.LoadParameters);

            await asyncOperation.ToUniTask(progress);

            loadingSceneData.LoadedScene = SceneManager.GetSceneByName(sceneLoadParams.SceneName);
            OnLoadingSceneComplete_handler(sceneLoadParams.SceneName, loadingSceneData.LoadedScene, sceneLoadParams.SetSceneActive);
        }

        return loadingSceneData;
    }

    IProgress<float> GetNewLoadingProgress(SceneLoadResult sceneLoadResult)
    {
        _serviceProgressTracking.RegisterLoadingProgress(sceneLoadResult.SceneLoadProgress);

        var progress = Progress.CreateOnlyValueChanged<float>(x =>
        {
            sceneLoadResult.SceneLoadProgress.SceneProgress = x;
            _serviceProgressTracking.UpdateProgress();
        });
        return progress;
    }
    
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