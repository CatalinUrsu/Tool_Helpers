using UnityEngine.SceneManagement;

namespace Helpers.Services
{
/// <summary>
/// Info about loaded scene. It contains:<br/>
/// <b>Scene</b> by itself, <br/>
/// <b>SceneLoadProgress</b> can be used to track loading progress (<b>SceneProgress</b> and <b>EntryPointProgress</b>).
/// </summary>
public class SceneLoadResult
{
    public Scene LoadedScene;
    public readonly SceneLoadProgress SceneLoadProgress = new();
}
}