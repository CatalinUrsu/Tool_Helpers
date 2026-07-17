using System;
using Cysharp.Threading.Tasks;

namespace Helpers.Services
{
public interface IProgressTrackingService
{
    bool UnloadsFinished { get; }
    event Action<float> OnUpdateProgress;
    event Action<string> OnUpdateLoadingTip;

    /// <summary>
    /// Reset Loading and Unloading processes. <br/>
    /// <b>IMPORTANT:</b> Call before show LoadingScreen or whenever need to reset progress (f.e. after hide LoadingScreen).
    /// </summary>
    void ResetProgress();
    
    /// <summary>
    /// Resister <b>SceneLoadProgress</b> to update total loading progress.
    /// </summary>
    void RegisterLoadingProgress(params SceneLoadProgress[] sceneLoadProgress);
    
    /// <summary>
    /// Register unload tasks and start track till all tasks are complete<br/>
    /// To check if unloads are finished, use <b><see cref="UnloadsFinished"/></b> field. 
    /// </summary>
    void RegisterUnloadProcesses(params UniTask[] unloadingTasks);
    
    /// <summary>
    /// Update progress of loading and unloading tasks and give the Avg of them <br/>
    /// </summary>
    void UpdateProgress();
    
    /// <summary>
    /// Update the tip on loading screen. (f.e. "Loading level 1", "Loading assets", etc.)
    /// </summary>
    void UpdateLoadingTip(string progressTip);
}
}