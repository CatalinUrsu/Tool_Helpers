using R3;
using System;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Helpers.Services
{
public class ProgressTrackingService : IProgressTrackingService
{
#region Fields

    public bool UnloadsFinished { get; private set; }
    public event Action<float> OnUpdateProgress;
    public event Action<string> OnUpdateLoadingTip;

    bool _loadsFinished;
    readonly List<SceneLoadProgress> _loadingProcesses = new();
    readonly List<UniTask> _unloadingTasks = new();
    readonly CompositeDisposable _loadDisposable = new();
    readonly CompositeDisposable _unloadDisposable = new();
    readonly TimeSpan _delayTimeSpan = TimeSpan.FromSeconds(.25f);

#endregion

#region Public methods

    public ProgressTrackingService() => ResetProgress();

    public void ResetProgress()
    {
        _loadingProcesses.Clear();
        _unloadingTasks.Clear();
        UnloadsFinished = true;
        _loadsFinished = false;

        OnUpdateProgress?.Invoke(0);
        OnUpdateLoadingTip?.Invoke(string.Empty);
    }
    
    public void RegisterLoadingProgress(SceneLoadProgress sceneLoadProgress) => _loadingProcesses.Add(sceneLoadProgress);

    public void RegisterLoadingProgress(params SceneLoadProgress[] sceneLoadProgress)
    {
        _loadingProcesses.AddRange(sceneLoadProgress);
        _loadDisposable.Clear();

        // Start observing progress updates at regular intervals until all unloads are finished
        Observable.Interval(_delayTimeSpan)
                  .TakeUntil(_ => _loadsFinished)
                  .Subscribe(_ => UpdateProgress())
                  .AddTo(_loadDisposable);
    }

    public void RegisterUnloadProcesses(params UniTask[] unloadingTasks)
    {
        // Check if the provided unloading tasks are null or empty
        if (unloadingTasks == null || unloadingTasks.Length == 0)
        {
            Debug.LogWarning("No unloading tasks provided.");
            return;
        }

        // Add each unloading task to the internal list
        foreach (var unloadTask in unloadingTasks)
            _unloadingTasks.Add(unloadTask);

        // Mark that unloads are not finished and clear any existing disposables 
        UnloadsFinished = false;
        _unloadDisposable.Clear();

        // Start observing progress updates at regular intervals until all unloads are finished
        Observable.Interval(_delayTimeSpan)
                  .TakeUntil(_ => UnloadsFinished)
                  .Subscribe(_ => UpdateProgress())
                  .AddTo(_unloadDisposable);
    }

    public void UpdateProgress()
    {
        var loadProgress = _loadingProcesses.Count != 0 ? GetLoadTasksProgress() : 0;
        var unloadProgress = _unloadingTasks.Count != 0 ? GetUnloadTasksProgress() : 1;
        OnUpdateProgress?.Invoke((loadProgress + unloadProgress) / 2);
        return;

        float GetLoadTasksProgress()
        {
            var progress = _loadingProcesses.Average(progress => progress.GetAvgProgress());
            _loadsFinished = Mathf.Approximately(progress, 1);

            return progress;
        }

        float GetUnloadTasksProgress()
        {
            var progress = _unloadingTasks.Select(task => task.GetAwaiter().IsCompleted ? 1 : 0)
                                          .Average(progress => (float)progress);
            UnloadsFinished = Mathf.Approximately(progress, 1);

            return progress;
        }
    }

    public void UpdateLoadingTip(string progressTip) => OnUpdateLoadingTip?.Invoke(progressTip);

#endregion
}
}