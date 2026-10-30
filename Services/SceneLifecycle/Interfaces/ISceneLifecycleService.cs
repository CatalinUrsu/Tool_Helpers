using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface ISceneLifecycleService
{
    UniTask<T> LoadAndInitScene<T>(SceneLoadParams sceneLoadParams) where T : class, IEntryPoint;
    UniTask DeinitAndUnloadScene(string sceneName, IEntryPoint entryPoint);
}
}
