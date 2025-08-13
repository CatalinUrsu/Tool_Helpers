using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface IServiceSceneLoader
{
    /// <summary>
    /// Async scenes loading implementation. Await till all scenes are loaded. <br/>
    /// It additionaly update loading tip and progress if is needed (need to set in <see cref="SceneLoadParams"/> parameter).
    /// </summary>
    /// <param name="sceneLoadParams">List of parameters (name, progress tip, use addressable or ResourceManager...)</param>
    /// <returns>List of info about loaded scenes, (Scene and LoadProgress)</returns>
    UniTask<SceneLoadResult[]> LoadScenes(params SceneLoadParams[] sceneLoadParams);
    
    /// <summary>
    /// Unload scene asynchronously by name. If automatically check how to unload it, using <b>Addressable</b> or <b>SceneManager</b>
    /// </summary>
    UniTask UnloadScene(string sceneName);
}
}