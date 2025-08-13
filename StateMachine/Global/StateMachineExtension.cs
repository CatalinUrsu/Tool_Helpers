using System.Linq;
using Helpers.Services;
using UnityEngine.SceneManagement;

namespace Helpers.StateMachine
{
public static class StateMachineExtension
{
    /// <summary>
    /// Find and return <b>ISceneEntry</b> component in FirstRootObject's children. RootObject's name must be "[Setup]". 
    /// </summary>
    public static IEntryPoint FindEntryPoint(this Scene scene) =>
        (from rootObject in scene.GetRootGameObjects()
         where rootObject.name.ToLower().Equals("[setup]")
         select rootObject.GetComponentInChildren<IEntryPoint>()).FirstOrDefault();

    /// <summary>
    /// Return scene by name from <see cref="SceneLoadResult"/> array.
    /// </summary>
    public static Scene GetScene(this SceneLoadResult[] sceneLoadResults, string name) => 
        sceneLoadResults.FirstOrDefault(result => result.LoadedScene.name.Equals(name))!.LoadedScene;
}
}