using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public class SceneLifecycleService: ISceneLifecycleService
{
    readonly IProgressTrackingService _progressTrackingService;
    readonly ISceneLoaderService _sceneLoaderService;
    readonly EntryPointRegistry _entryPointRegistry;

    public SceneLifecycleService(IProgressTrackingService progressTrackingService,
                                 ISceneLoaderService sceneLoaderService,
                                 EntryPointRegistry entryPointRegistry)
    {
        _progressTrackingService = progressTrackingService;
        _sceneLoaderService = sceneLoaderService;
        _entryPointRegistry = entryPointRegistry;
    }

    public async UniTask<T> LoadAndInitScene<T>(SceneLoadParams sceneLoadParams) where T : class, IEntryPoint
    {
        var sceneLoadResult = new SceneLoadResult();
        T entryPoint = null;
        
        _progressTrackingService.RegisterLoadingProgress(sceneLoadResult.SceneLoadProgress);
        await UniTask.WhenAll(LoadAndInit(), WaitUnloadingTask());

        sceneLoadResult.SceneLoadProgress.SetupProgress = 1;
        return entryPoint;

        async UniTask LoadAndInit()
        {
            await _sceneLoaderService.LoadScene(sceneLoadParams, sceneLoadResult);
            
            // Scene registers its entry point while loading (see EntryPointRegistry)
            entryPoint = _entryPointRegistry.Get<T>();
            await entryPoint.Init(_progressTrackingService, sceneLoadResult.SceneLoadProgress);
        }
        
        UniTask WaitUnloadingTask() => UniTask.WaitUntil(() => _progressTrackingService.UnloadsFinished);
    }

    public async UniTask DeinitAndUnloadScene(string sceneName, IEntryPoint entryPoint)
    {
        await entryPoint.DeInit();

        var unloadSceneTask = _sceneLoaderService.UnloadScene(sceneName)
                                                 .ContinueWith(UnloadUnusedResources);

        _progressTrackingService.RegisterUnloadProcesses(unloadSceneTask);
    }

    static UniTask UnloadUnusedResources() => Resources.UnloadUnusedAssets().ToUniTask();
}
}
