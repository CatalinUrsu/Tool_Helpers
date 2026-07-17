using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface ISceneLoaderService
{
    /// <summary>
    /// Async scene loading implementation. Await till scene is loaded. <br/>
    /// It additionally update loading tip and progress if is needed (need to set in <see cref="SceneLoadParams"/> parameter).
    /// </summary>
    /// <param name="sceneLoadParams">List of parameters (name, progress tip, use addressable or ResourceManager...)</param>
    /// <param name="loadResult">Out Result of loaded scene, can be used to track loading progress</param>
    UniTask LoadScene(SceneLoadParams sceneLoadParams, SceneLoadResult loadResult);
    
    /// <summary>
    /// Unload scene asynchronously by name. If automatically check how to unload it, using <b>Addressable</b> or <b>SceneManager</b>
    /// </summary>
    UniTask UnloadScene(string sceneName);
}
}