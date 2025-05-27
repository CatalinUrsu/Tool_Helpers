using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface IServiceSceneLoader
{
    /// <summary>
    /// Async scenes loading using Addressable or ResourceManager. It automatically update loading prompt and progress if is needed (need to set in <see cref="SceneLoadParams"/> parameter).
    /// </summary>
    /// <param name="sceneLoadParams">List of parameters (name, progress prompt, if scene IsAddressable...)</param>
    /// <returns><b>Scene</b>: scene by  itself. <b>SceneLoadProgress</b>: can be used to track progress of objects setup after scene load.</returns>
    UniTask<SceneLoadResult[]> LoadScenes(params SceneLoadParams[] sceneLoadParams);
    
    /// <summary>
    /// Unload scene asynchronously by name. If scene was cached in dictionary, then use <b>Addressable</b>
    /// If not (ex: init scene), then it is removed using <b>SceneManager</b>
    /// </summary>
    UniTask UnloadScene(string sceneName);
}
}