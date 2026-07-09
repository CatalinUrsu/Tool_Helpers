using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Helpers.Services
{
public class SplashScreenService : ISplashScreenService
{
    ISplashScreen _currentSplashScreen;
    readonly Dictionary<string, ISplashScreen> _splashScreens = new();
    readonly IProgressTrackingService _progressTrackingService;

    public SplashScreenService(IProgressTrackingService progressTrackingService) => _progressTrackingService = progressTrackingService;

    public void RegisterSplashScreen(string key, ISplashScreen splashScreen)
    {
        if (_splashScreens.TryAdd(key, splashScreen)) return;
        Debug.LogError($"Splash screen with key '{key}' is already registered.");
    }

    public async UniTask Show(string key, bool skipAnimation)
    {
        if (_splashScreens.TryGetValue(key, out var splashScreen))
        {
            _currentSplashScreen = splashScreen;
            await _currentSplashScreen.ShowPanel(skipAnimation);
        }
    }

    public async UniTask Hide()
    {
        if (_currentSplashScreen == null)
        {
            Debug.LogWarning("No current splash screen to hide.");
            return;
        }
        
        await _currentSplashScreen.HidePanel();
        _currentSplashScreen = null;
        _progressTrackingService.ResetProgress();
    }
}
}