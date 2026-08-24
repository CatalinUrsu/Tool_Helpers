using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface ISceneLifecycleService
{
    UniTask LoadAndInitScene(SceneLoadParams sceneLoadParams, ISceneContext sceneContext);
    UniTask DeinitAndUnloadScene(string sceneName, ISceneContext sceneContext);
}
}