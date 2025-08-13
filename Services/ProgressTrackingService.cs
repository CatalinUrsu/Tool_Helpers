using R3;
using System;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Helpers.Services
{
public class ProgressTrackingService : IServiceProgressTracking
{
#region Fields

    public int LoadProgressCount { get; set; }
    public bool UnloadsAreFinished { get; private set; }
    public event Action<float> OnUpdateProgress;
    public event Action<string> OnUpdateLoadingTip;

    readonly List<SceneLoadProgress> _loadingProcesses = new();
    readonly List<UniTask> _unloadingTasks = new();
    readonly CompositeDisposable _disposable = new();
    readonly TimeSpan _delayTimeSpan = TimeSpan.FromSeconds(.25f);

#endregion

#region Public methods

    public ProgressTrackingService() => ResetProgress();

    public void ResetProgress()
    {
        _loadingProcesses.Clear();
        _unloadingTasks.Clear();
        LoadProgressCount = 0;
        UnloadsAreFinished = true;

        OnUpdateProgress?.Invoke(0);
        OnUpdateLoadingTip?.Invoke(string.Empty);
    }
    
    public void RegisterLoadingProgress(SceneLoadProgress sceneLoadProgress) => _loadingProcesses.Add(sceneLoadProgress);

    public void RegisterUnloadProcesses(params UniTask[] unloadingTasks)
    {
        if (unloadingTasks == null || unloadingTasks.Length == 0)
        {
            Debug.LogWarning("No unloading tasks provided.");
            return;
        }

        foreach (var unloadTask in unloadingTasks)
            _unloadingTasks.Add(unloadTask);

        UnloadsAreFinished = false;
        _disposable.Clear();
        Observable.Interval(_delayTimeSpan)
                  .TakeUntil(_ => UnloadsAreFinished)
                  .Subscribe(_ => UpdateProgress())
                  .AddTo(_disposable);
    }

    public void UpdateProgress()
    {
        var unloadProgress = _unloadingTasks.Count != 0 ? GetUnloadTasksProgress() : 1;
        var loadProgress = LoadProgressCount != 0 ? GetLoadTasksProgress() : 0;
        OnUpdateProgress?.Invoke((loadProgress + unloadProgress) / 2);
        return;

        float GetLoadTasksProgress() => _loadingProcesses.Sum(progress => progress.GetAvgProgress()) / LoadProgressCount;

        float GetUnloadTasksProgress()
        {
            var progress = _unloadingTasks.Select(task => task.GetAwaiter().IsCompleted ? 1 : 0)
                                                .Average(progress => (float)progress);
            UnloadsAreFinished = Mathf.Approximately(progress, 1);

            return progress;
        }
    }

    public void UpdateLoadingTip(string progressTip) => OnUpdateLoadingTip?.Invoke(progressTip);

#endregion
}
}