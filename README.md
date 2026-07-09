<h1 align="center">
Helpers
</h1>
<div align="center">
Additional helpful scripts for unity, that are designed to be used in multiple projects
</div>

# Contents:
- [Install ✅](#install-)
- [Audio 🔊](#audio-)
- [PoolSystem 🔄](#pool-system-)
  - [Pool ♻️](#pool-)
  - [PooledObject 📦](#pooled-object-)
  - [Factory 🏭](#factory-)
- [SaveSystem 💾](#save-system-)
- [UI 📺](#ui-)
  - [DummyImage 🖼️](#dummy-image-)
  - [InputManager 📐](#input-manager-)
  - [ButtonHelper 🆒](#button-helper-)
- [State Machine 🕹️](#state-machine-)
- [Services 🧰](#services-)
  - [Camera Service 🎥](#camera-service-)
  - [Scene Loader Service ⏱️](#scene-loader-service-)
  - [Loading Progress Tracking Service 📼](#loading-progress-tracking-service-)
  - [Splash Screen Service 🧩](#splash-screen-service-)
- [Notes](#notes)


# Install ✅

1. Open Package Manager window (Window | Package Manager)
1. Click `+` button on the upper-left of a window, and select "Add package from git URL..."
1. Enter the following URL and click `Add` button

```
https://github.com/CatalinUrsu/Tool_Helpers.git
```

# Audio 🔊
The Audio module contains helper scripts for working with FMOD events, banks, and reusable event instances.
> **Note:** Audio scripts are placed under a separate assembly definition and are compiled only when the
> `FMOD_INSTALLED` assembly definition symbol is added to the project.
- [BankLoader](Audio/BankLoader.cs) loads an FMOD bank from an <b>Addressable `TextAsset`</b> and unloads it when it is no longer needed.
- [FmodExtensions](Audio/FmodExtensions.cs) provides shortcuts for playing one-shot sounds, creating event instances, setting parameters,
  and releasing cached instances by scene.
- [FactoryFmodEvents](Audio/FactoryFmodEvents.cs) creates pooled FMOD event instances, which is useful for frequently played sounds such as
  shots, hits, impacts, or UI feedback. <b>Usage:</b>
- [FmodSettingsOverrideOnPlay](Audio/FmodSettingsOverrideOnPlay.cs) adjusts FMOD import settings in Editor play mode to simplify local testing with
  Addressables.

```csharp
using FMOD.Studio;
using FMODUnity;
using Helpers.Audio;
using Helpers.PoolSystem;

EventReference soundReference;

// Create pool for often-used sounds.
var hitSoundPool = new FactoryFmodEvents.Builder(soundReference)
                   .SetPreloadCount(5)        // Create 5 instances on pool init.
                   .SetMaxCount(7)            // Allow up to 7 active instances.
                   .Set3DAttributes(true)     // Enable spatial sound support.
                   .Build();

// Get instance from pool.
EventInstance instance1 = hitSoundPool.Get();

// Return instance to pool manually if needed.
hitSoundPool.Release(instance1);

// Play simple one-shot sound.
soundReference.PlayOneShot();

// Create standalone instance.
EventInstance instance2 = soundReference.GetInstance();

// Set float parameter.
instance2.SetParameter("paramName1", 123);

// Set labeled parameter.
instance2.SetParameter("paramName2", "NewValue");

// Stop and release standalone instance.
instance2.ReleaseInstance();

// Create instance cached by scene name.
instance2 = soundReference.GetInstance("SceneName");

// Release all instances cached for this scene.
FmodExtensions.ReleaseSceneInstances("SceneName");
```


# <br>Pool System 🔄
<b>PoolSystem</b> is used to reuse objects instead of creating and destroying them every time.  
It is useful for bullets, enemies, effects, sounds, UI elements, or any object that appears often.
<br><img src="https://i.postimg.cc/mrcDqKzR/Pool.png" alt="Pool" width="370">

<b>Main parts:</b>
- [Pool](Global/PoolSystem/Pool.cs) stores inactive objects and gives them back when needed.
- [PooledObject](Global/PoolSystem/PooledObject.cs) - Base class for pooled scene objects. 
  - `Init` - used to set ReturnToPool action and some configs (optional)
  - `Set` - used to set config data when its needed (f.e. - each time when iss taken from pool 
  - Objects can also return themselves to pool by calling `OnReleaseToPool_raise()`.
- [Factory](Global/PoolSystem/Factory.cs) creates and configures pools in a simple builder style.

```csharp using UnityEngine; using Source.Gameplay; using Helpers.PoolSystem;
[SerializeField] Bullet _bulletPrefab;
[SerializeField] Transform _poolActive;
[SerializeField] Transform _poolInactive;
Pool<PooledObject> _bulletPool;

void TestPool()
{
var bulletPower = 10f;

    // Create pool for often-used objects.
    _bulletPool = new Factory.Builder(_bulletPrefab)
                  .SetConfig(bulletPower)               // Pass shared init data to created objects.
                  .SetParents(_poolActive, _poolInactive) // Move active/inactive objects under different parents.
                  .SetPreloadCount(10)                  // Create 10 objects on pool init.
                  .SetMaxCount(15)                      // Keep up to 15 inactive objects.
                  .Build();

    // Get objects from pool.
    PooledObject bullet = _bulletPool.Get();
    PooledObject bullet2 = _bulletPool.Get();

    // Apply post-spawn setup, if object needs it.
    bullet.Set();

    // Return specific object to pool.
    _bulletPool.Release(bullet);

    // Return all active objects to pool.
    _bulletPool.ReleaseAll();

    // Destroy pooled objects and clear pool data.
    _bulletPool.Clear();
}
```

## PooledObject 📦
Base class for pooled scene objects.  
Override `Init` to read shared config once, and `Set` to reset object state each time it is taken from pool.

Objects can also return themselves to pool by calling `OnReleaseToPool_raise()`.
```

csharp using System; using UnityEngine; using Helpers.PoolSystem;
public class Bullet : PooledObject { public override PooledObject Init(ActiononReleaseToPool, object config = null) { // Read shared config here. return base.Init(onReleaseToPool, config); }
public override void Set(object config = null)
{
    // Reset state here before using object again.
}

void OnTriggerEnter2D(Collider2D other)
{
    // Return this object to pool.
    OnReleaseToPool_raise();
}
}
``` 

## Factory 🏭
Factory creates pools for `PooledObject` prefabs.  
Use it when pool creation logic is similar for many objects, like bullets, enemies, and effects.

<br><img src="https://i.postimg.cc/T1XX24HN/Factory.png" alt="Factory" width="800">


# <br>Save System 💾
Generic and common easy to use save system that can save or load files from device. Also it has byte encoding, it’s not necessary for this project,
BUT I WANTED IT !!!
```csharp
using System;
using Constants;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Source.Session
{
public class SaveSystem
{
    public void Save<T>(string filePath, T sessionElement)
    {
        if (Directory.Exists(ConstSession.SAVES_FOLDER_PATH) == false)
            Directory.CreateDirectory(ConstSession.SAVES_FOLDER_PATH);

        using (FileStream fs = File.Open(filePath, FileMode.Create))
        {
            BinaryWriter writer = new BinaryWriter(fs);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sessionElement));
            writer.Write(Convert.ToBase64String(plainTextBytes));
            writer.Flush();
        }
    }
    
    public T Load<T>(string filePath) where T: class
    {
        if (!Directory.Exists(ConstSession.SAVES_FOLDER_PATH) || !File.Exists(filePath)) return null;

        using (FileStream fs = File.Open(filePath, FileMode.Open))
        {
            BinaryReader reader = new BinaryReader(fs);
            byte[] encodedBytes = Convert.FromBase64String(reader.ReadString());
            var value = Encoding.UTF8.GetString(encodedBytes);
            return JsonConvert.DeserializeObject<T>(value);;
        }
    }

    public static void ResetSaves()
    {
        if (Directory.Exists(ConstSession.SAVES_FOLDER_PATH))
            Directory.Delete(ConstSession.SAVES_FOLDER_PATH, true);
    }
}
}
```


# <br>UI 📺
## Dummy Image 🖼️
just an empty image, good solution for cases when it need to get raycast but without any UI elements

## Input Manager 📐
I used it to handle UI LockInput, but instead of disabling the whole UI from EventSystem, I decided to create a Canvas on the whole screen with
DummyImage that will block input on the UI. With this implementation we still can tap on several UI elements if their sorting order is higher than on
BlockInput one.
<br>I also added solution for checking PointerOverUI, in this project I didn’t used it, but my experience told me that it must be here.

## Button Helper 🆒
Just an implementation of pointers interfaces that raise event when it’s needed.


# <br>State Machine 🕹️
StateMachine script is used to manage the states of an object, controlling its behavior based on its current state.
It helps in organizing code for different states (init, menu, gameplay, loading, etc). This implementation is based on async changing state.
Firstly it wait till the current state is finished, then it changes to the new state.
```csharp
using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace StateMachine
{
[Serializable]
public class StatesMachine
{
    readonly Dictionary<Type, IState> _states;
    protected IState currentState { get; set; }

    public StatesMachine(params IState[] states)
    {
        _states = new Dictionary<Type, IState>();

        foreach (var state in states)
        {
            if (_states.ContainsKey(state.GetType()))
                continue;

            state.StatesMachine = this;
            _states.Add(state.GetType(), state);
        }
    }
    
    public virtual async UniTask Enter<TState>(Action onLoaded = null) where TState : class, IStateEnter
    {
        var state = await ChangeState<TState>();
        state.Enter().Forget();
    }

    public virtual async UniTask Enter<TState, TPayload>(TPayload payload) where TState : class, IStateEnterPayload<TPayload>
    {
        var state = await ChangeState<TState>();
        state.Enter(payload).Forget();
    }

    protected virtual async UniTask<TState> ChangeState<TState>() where TState : class, IState
    {
        if (currentState != null)
            await currentState.Exit();

        var state = GetState<TState>();
        currentState = state;
        return state;
    }

    protected virtual TState GetState<TState>() where TState : class, IState => _states[typeof(TState)] as TState;
}
}
```


# Services 🧰
Beside the State Machine logic, it also contains some services that can be used in any project, like:
<i>
- [Camera Service](Services/CameraService.cs)
- [Scene Loader](Services/SceneLoaderService.cs)
- [Loading Progress Service](Services/LoadingProgressService.cs)
- [Splash Screen Service](Services/SplashScreenService.cs)
  </i>

## Camera Service 🎥
Is used to get access to main camera of to any camera by tag or name. Useful for getting camera for some canvas with render mode set to
screen space camera.

## Scene Loader Service ⏱️
Service used to load and unload any scene using SceneManager or Addressable system. For tracking loading progress, it uses
<b>LoadingProgressService</b> and <i><b><a href="https://github.com/Cysharp/UniTask?tab=readme-ov-file#progress">IProgress</a></b></i>
interface implementation from <i><b><a href="https://github.com/Cysharp/UniTask">UniTask</a></b></i> repo.
<br>As a parameter for loading scene it used [Scene Load Params](SceneLoadInfo/SceneLoadParams.cs), that is just a struct with some options for loading
scene and that can be created using vert friendly linq-style Builder pattern.
<br>As a result of loading scene, it returns [Scene Load Result](SceneLoadInfo/SceneLoadResult.cs) that contains some info about loading scene.

## Loading Progress Tracking Service 📼
This service is used to track loading and unloading of scenes and content on them. For this, it uses [Scene Load Progress](SceneLoadInfo/SceneLoadProgress.cs)
from [Scene Load Result](SceneLoadInfo/SceneLoadResult.cs), this class contains float values for scene loading progress and content loading progress.
<br><b>NOTE:</b> For correct working of loading tracking, need to set total number of scenes and content that will be loaded in the beginning of loading process.

## Splash Screen Service 🧩
Service used to show splash screen during loadding process. It uses [ISplashScreen](Interfaces/ISplashScreen.cs) interface for showing and hiding splash screen.
```csharp
using Cysharp.Threading.Tasks;

namespace StateMachine
{
public class SplashScreenService : IServiceSplashScreen
{
    ISplashScreen _splashScreen;
    IServiceProgressTracking _loadingProgressService;

    public SplashScreenService(IServiceProgressTracking loadingProgressService) => _loadingProgressService = loadingProgressService;

    public void RegisterSplashScreen(ISplashScreen panel) => _splashScreen = panel;

    public async UniTask ShowPage(bool skipAnimation = false) => await _splashScreen.ShowPanel(skipAnimation);

    public async UniTask HidePage()
    {
        await _splashScreen.HidePanel();
        _loadingProgressService.ResetProgress();
    }
}
}
```


# Notes:
If use this package in project for first time, it may rise some errors. This is caused because of large number of dependencies, including <b>Git dependencies</b>,
which are loaded via script on project open  (if these packages are not already in manifest file). Click <b>"Ignore"</b> and wait till unity will load everything.
<br><img src="https://i.postimg.cc/m2vRBf3g/Unity.png" alt="Unity Error" width="300">
<br><br>If the issue is not resolved because Fmod is not fully imported, remove old FMOD and restart project. This will force unity to load all dependencies again.