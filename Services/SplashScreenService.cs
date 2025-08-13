using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Helpers.Services
{
public class SplashScreenService : IServiceSplashScreen
{
    ISplashScreen _currentSplashScreen;
    readonly Dictionary<string, ISplashScreen> _splashScreens = new();
    readonly IServiceProgressTracking _serviceProgressTracking;

    public SplashScreenService(IServiceProgressTracking serviceProgressTracking) => _serviceProgressTracking = serviceProgressTracking;

    public void RegisterSplashScreen(string key, ISplashScreen splashScreen)
    {
        if (_splashScreens.TryAdd(key, splashScreen)) return;
        Debug.LogError($"Splash screen with key '{key}' is already registered.");
    }

    public async UniTask ShowPage(string key, bool skipAnimation = false)
    {
        if (_splashScreens.TryGetValue(key, out var splashScreen))
        {
            _currentSplashScreen = splashScreen;
            await _currentSplashScreen.ShowPanel(skipAnimation);
        }
    }

    public async UniTask HidePage()
    {
        if (_currentSplashScreen == null)
        {
            Debug.LogWarning("No current splash screen to hide.");
            return;
        }
        
        await _currentSplashScreen.HidePanel();
        _currentSplashScreen = null;
        _serviceProgressTracking.ResetProgress();
    }
}
}