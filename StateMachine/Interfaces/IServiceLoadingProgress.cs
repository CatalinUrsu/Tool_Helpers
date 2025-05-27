using System;
using Cysharp.Threading.Tasks;

namespace Helpers.StateMachine
{
public interface IServiceLoadingProgress
{
    int LoadProgressCount { get; set; }
    bool UnloadsAreFinished { get; }
    event Action<float> OnUpdateProgress;
    event Action<string> OnUpdateLoadingPrompt;

    void ResetProgress();
    
    /// <summary>
    /// Store <b>loadings</b> at runtime to have ref to original value during whole loading
    /// Store LoadingProgress to update loading bar. Should be called in <b>StateEnter</b> of each scene
    /// </summary>
    void RegisterLoadingProgress(SceneLoadProgress sceneLoadProgress);

    /// <summary>
    /// Store <b>UnloadingTasks</b>. It uses UnitaskVoid to await till tasks are finished, but don't block thread.
    /// To check if all tasks are finished use <see cref="UnloadsAreFinished"/> property.
    /// <b> Useful when it's needed to unload and load objects in parallel. </b> 
    /// </summary>
    UniTaskVoid RegisterUnloadingTasks(params UniTask[] unloadingTasks);
    
    /// <summary>
    /// Call when need to calculate the progress <b>Avg(Load+Unload)</b> and updated loading bar
    /// </summary>
    void UpdateProgress();
    
    /// <summary>
    /// Update the hint (prompt) on loading screen
    /// </summary>
    void UpdateLoadingPrompt(string progressPrompt);
}
}