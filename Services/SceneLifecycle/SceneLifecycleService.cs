using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public class SceneLifecycleService: ISceneLifecycleService
{
    readonly IProgressTrackingService _progressTrackingService;
    readonly ISceneLoaderService _sceneLoaderService;

    public SceneLifecycleService(IProgressTrackingService progressTrackingService, ISceneLoaderService sceneLoaderService)
    {
        _progressTrackingService = progressTrackingService;
        _sceneLoaderService = sceneLoaderService;
    }

    public async UniTask LoadAndInitScene(SceneLoadParams sceneLoadParams, ISceneContext sceneContext)
    {
        var sceneLoadResult = new SceneLoadResult();
        _progressTrackingService.RegisterLoadingProgress(sceneLoadResult.SceneLoadProgress);

        var waitUnloadingTask = UniTask.WaitUntil(() => _progressTrackingService.UnloadsFinished);
        var loadSceneTask = GetSceneLoadTask(sceneLoadParams, sceneContext, sceneLoadResult);

        await UniTask.WhenAll(loadSceneTask, waitUnloadingTask);

        sceneLoadResult.SceneLoadProgress.SetupProgress = 1;
    }

    public async UniTask DeinitAndUnloadScene(string sceneName, ISceneContext sceneContext)
    {
        await sceneContext.DeInit();

        var unloadSceneTask = _sceneLoaderService.UnloadScene(sceneName)
                                                 .ContinueWith(UnloadUnusedResources);

        _progressTrackingService.RegisterUnloadProcesses(unloadSceneTask);
    }

    UniTask GetSceneLoadTask(SceneLoadParams sceneLoadParams, ISceneContext sceneContext, SceneLoadResult sceneLoadResult)
    {
        return _sceneLoaderService.LoadScene(sceneLoadParams, sceneLoadResult)
                                  .ContinueWith(GetSceneContextInit);

        UniTask GetSceneContextInit() => sceneContext.Init(_progressTrackingService, sceneLoadResult.SceneLoadProgress);
    }

    static UniTask UnloadUnusedResources() => Resources.UnloadUnusedAssets().ToUniTask();
}
}