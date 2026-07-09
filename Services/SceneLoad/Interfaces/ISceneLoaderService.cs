using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface ISceneLoaderService
{
    /// <summary>
    /// Async scene loading implementation. Await till scene is loaded. <br/>
    /// It additionaly update loading tip and progress if is needed (need to set in <see cref="SceneLoadParams"/> parameter).
    /// </summary>
    /// <param name="sceneLoadParams">List of parameters (name, progress tip, use addressable or ResourceManager...)</param>
    /// <returns>List of info about loaded scene, (Scene and LoadProgress)</returns>
    UniTask<SceneLoadResult> LoadScene(SceneLoadParams sceneLoadParams);
    
    /// <summary>
    /// Unload scene asynchronously by name. If automatically check how to unload it, using <b>Addressable</b> or <b>SceneManager</b>
    /// </summary>
    UniTask UnloadScene(string sceneName);
}
}