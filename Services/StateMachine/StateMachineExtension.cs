using System.Linq;
using Helpers.Services;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Helpers.Services
{
public static class StateMachineExtension
{
    /// <summary>
    /// Return scene by name from <see cref="SceneLoadResult"/> array.
    /// </summary>
    public static Scene GetScene(this IEnumerable<SceneLoadResult> sceneLoadResults, string name) => 
        sceneLoadResults.FirstOrDefault(result => result.LoadedScene.name.Equals(name))!.LoadedScene;
}
}