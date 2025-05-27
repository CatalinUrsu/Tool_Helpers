using UnityEngine.SceneManagement;

namespace Helpers.StateMachine
{
public class SceneLoadResult
{
    public Scene LoadedScene;
    public readonly SceneLoadProgress SceneLoadProgress = new();
}
}