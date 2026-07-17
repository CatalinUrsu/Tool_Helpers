<h1 align="center">Helpers</h1>

---

## Overview

Helpers is a collection of reusable modules extracted from production projects.
The package focuses on solving common gameplay and infrastructure problems while
keeping the API lightweight, modular and easy to integrate.</br>
The repository contains independent systems that can be used together or
separately depending on project requirements.

## Features

- Runtime Audio infrastructure (FMOD)
- Generic Object Pooling
- Save System
- Async State Machine
- Scene Loading Services
- Loading Progress Tracking
- Splash Screen Management
- UI Components
- Common Utilities

## Installation

1. Open Package Manager window (Window | Package Manager)
1. Click `+` button on the upper-left of a window, and select "Add package from git URL..."
1. Enter the following URL and click `Add` button

```
https://github.com/CatalinUrsu/Tool_Helpers.git
```

---

## Modules

### Runtime

- [Audio 🔊](#audio-)
- [Pooling ♻️](#pooling-)
- [Save System 💾](#save-system-)
- [UI 🖥️](#ui-)

### Services

- [State Machine 🔄](#state-machine-)
- [Camera Service 📷](#camera-service-)
- [Scene Loader Service 🚪](#scene-loader-service-)

---

# Audio 🔊
Helpers for working with FMOD events, banks and reusable event instances.

> **Note:**</br>
> Audio scripts are compiled only when the `FMOD_INSTALLED`
> assembly definition symbol is present.

## Components

- [BankLoader](Audio/BankLoader.cs) - Runtime FMOD bank loading from Addressable `TextAsset`s.
- [FmodExtensions](Audio/FmodExtensions.cs) - Extension methods for playback, parameter handling and cached instances.
- [FactoryFmodEvents](Audio/FactoryFmodEvents.cs) - Object pooling for reusable FMOD event instances.
- [FmodSettingsOverrideOnPlay](Audio/FmodSettingsOverrideOnPlay.cs) - Editor helper for Addressables testing.

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

---

# Pooling ♻️

Reusable object pooling system for reducing runtime allocations and avoiding
frequent object instantiation.</br>
Factories are reusable and can preload instances during initialization,
limit the maximum number of active objects and expand automatically when
configured to do so.

## Components

- [Factory](Global/PoolSystem/Factory.cs) — Base factory for creating and managing pooled objects.
- [Pool](Global/PoolSystem/Pool.cs) — Generic pool implementation with configurable capacity.
- [PooledObject](Global/PoolSystem/PooledObject.cs) - Base class for pooled scene objects.
  - `Init` - used to set ReturnToPool action and some configs (optional)
  - `Set` - used to set config data when its needed (f.e. - each time when iss taken from pool
  - Objects can also return themselves to pool by calling `OnReleaseToPool_raise()`.

## Example

```csharp
// Create a GameObject pool.
FactoryGO bulletPool = new FactoryGO.Builder(bulletPrefab)
    .SetPreloadCount(20)
    .SetMaxCount(50)
    .Build();

// Retrieve an instance.
GameObject bullet = bulletPool.Get();

// Use the object...
bullet.transform.position = spawnPoint.position;

// Return it back to the pool.
bulletPool.Release(bullet);
```

Objects can also return themselves automatically by using the `PoolObject`
component.

```csharp
public class Bullet : MonoBehaviour
{
    [SerializeField] private PoolObject poolObject;

    private void OnCollisionEnter(Collision collision)
    {
        poolObject.Release();
    }
}
```

---

# Save System 💾

Lightweight persistence helpers for saving and loading application data.

```csharp
[Serializable]
public class PlayerData
{
    public int Coins;
    public int Level;
}

PlayerData data = new()
{
    Coins = 250,
    Level = 8
};

// Save data.
SaveManager.Save("PlayerData", data);

// Load data.
if (!SaveSystem.TryLoad("PlayerDataPath", out playerData))
            playerData = new PlayerData();

// Delete save.
SaveManager.Delete("PlayerDataPath");
```

---

# UI 🖥️

Collection of reusable UI components and helpers designed to simplify common
user interface interactions.

## Components

- [DummyImage](UI/DummyImage.cs) — Invisible raycast target for UI layouts.
- [InputManager](UI/InputManager.cs) — Help to lock/unlock input using delegate, check PointOver needed object
- [Dropdown](UI/DropdownHelper.cs) — Extended dropdown with additional functionality:
  - Easy and well documented API
  - Reactive workflow (``using R3 collection``)
  - Updated visual interface
  - Possibility to add/remove/select multiple items
  - Suport Navigation (gamepad, keyboard...)
- [RTEditor](UI/Editor/RTEditor.cs) — Extended functionality of RectTransform:
  - Anchor UI element to corners
  - Mirror position on X and Y axis

## Example

```csharp
//UI input will be blocked during whole operationo
using (InputManager.Instance.LockInputSystem())
{
    // Do some stuff
}
```
---

# State Machine 🔄

Generic asynchronous finite state machine for managing application and gameplay
states. </br>
States can optionally implement only the lifecycle interfaces they require.
The state machine automatically invokes supported callbacks during state
transitions.

## Components

- [StateMachine](Services/StateMachine/StatesMachine.cs) — Manages state registration and transitions.
- [IState](Services/StateMachine/Interfaces/IState.cs) — Base interface for all states.
- [IStateEnter](Services/StateMachine/Interfaces/IStateEnter.cs) — Default state interface.
- [IStateEnterPayload](Services/StateMachine/Interfaces/IStateEnterPayload.cs) — Generic state interface. Used for custom state with some config/data.

## Example

```csharp
public class StateMenu : IEnterState
{
    public async UniTask Enter()
    {
        // Enter stuff here
    }

    public async UniTask Exit()
    {
        // Exit stuff here
    }
}

public class StateGameplay : IStateEnterPayload<bool>
{
    public async UniTask Enter(bool payload)
    {
        // Enter stuff here
    }

    public async UniTask Exit()
    {
        // Exit stuff here
    }
}
```

```csharp
// Create list of states (use base IState interface)
var states = new IState[]
{
    new StateInit(),
    new StateMenu(_serviceOne, _serviceTwo),
    new StateGameplay<bool>(_serviceOne, _serviceThree)
};

// Create StateMachine instance and assign needed states
_stateMachine = new StatesMachine(states);

//Enter some state 
_stateMachine.Enter<StateInit>().GetAwaiter();

//Enter Payload state
_stateMachine.Enter<StateGameplay, bool>(false).GetAwaiter();
```

---

# Camera Service 📷

Provides centralized registration and access to cameras at runtime.</br>
When using the Universal Render Pipeline, `CameraServiceUniversal`
can automatically add registered cameras to the main camera stack.

## Components

- [CameraService](Services/Camera/CameraService.cs) — Register/Unregister and cache cameras. [Recommended to use via [ICameraService](Services/Camera/Interfaces/ICameraService.cs) interface]
- [CameraServiceUniversal](Services/Camera/CameraServiceUniversal.cs)**CameraServiceUniversal** — Extends `CameraService` with automatic URP camera stack management.

## Example

```csharp
// Register the main camera.
_cameraService.RegisterMainCamera(mainCamera);

// Register an additional camera.
_cameraService.RegisterCamera("Minimap", minimapCamera);

cameraService.RegisterCamera("Overlay", overlayCamera, addToStack: true);

// Retrieve the main camera.
Camera camera = cameraService.GetMainCamera();

// Retrieve a registered camera.
Camera minimap = cameraService.GetCameraByKey("Minimap");

// UnRegister camera (and remove from stach if is needed)
Camera minimap = cameraService.UnregisterCamera("Minimap");
```

---

# Scene Loader Service 🚪

Provides a unified API for loading and unloading scenes from both Build Settings
and Addressables.

> **Note:**</br>
> The same API supports both <b>Buildin</b> and <b>Addressable</b> scenes (Load/Cache/Unload)</br>
> Progress can be tracked through [IServiceProgressTracking](Services/ProgressTracking/Interfaces/IProgressTrackingService.cs)

## Features

- Build Settings and Addressables scene loading
- Async/await API (UniTask)
- Loading progress reporting
- Optional active scene switching
- Loading screen tip support

## Components

- [SceneLoaderService](Services/SceneLoad/SceneLoaderService.cs) - Handles asynchronous scene loading and unloading
(Recomended to use via [ISceneLoaderService](Services/SceneLoad/Interfaces/ISceneLoaderService.cs))
- [SceneLoadParams](Services/SceneLoad/SceneLoadInfo/SceneLoadParams.cs) - Store configurations for scene loading (using Builder pattern)
- [SceneLoadResult](Services/SceneLoad/SceneLoadInfo/SceneLoadResult.cs) - Store <b>Loaded Scene</b> and <b>[SceneLoadProgress](Services/SceneLoad/SceneLoadInfo/SceneLoadProgress.cs)</b> (Can be used for [ProgressTrackingService](Services/ProgressTracking/ProgressTrackingService.cs))
- [ProgressTrackingService](Services/ProgressTracking/ProgressTrackingService.cs) — Aggregates progress from registered loading operations.

## Example

```csharp
readonly ISceneLoaderService _sceneLoaderService;
readonly ISplashScreenService _splashScreenService;
readonly IProgressTrackingService _progressTrackingService;

async UniTask LoadNeededScenes()
{
    const string menuSceneName = "Menu";
    const string gameSceneName = "Game";

    //(Optional) Create Dict of SceneLoadResult for easier to get value by ID (sceneName)
    var sceneLoadResults = new Dictionary<string, SceneLoadResult>
    {
        { menuSceneName, new SceneLoadResult() },
        { gameSceneName, new SceneLoadResult() }
    };

    // Create LoadParams for all needed scenes
    var menuSceneLoadParams = new SceneLoadParams.Builder(menuSceneName)
                              .SetIsAddressable(true)
                              .SetTip("Load Menu Scene")
                              .Build();
    var gameSceneLoadParams = new SceneLoadParams.Builder(menuSceneName)
                              .SetIsAddressable(true)
                              .SetActiveOnLoad(true)
                              .SetTip("Load Game Scene")
                              .Build();

    //Take SceneLoadProgress from all SceneLoadResults
    var scenesLoadProgress = sceneLoadResults.Values.Select(result => result.SceneLoadProgress).ToArray();

    // Get tasks of scene loading, give sceneLoadResult for each 
    var menuLoadTask = _sceneLoaderService.LoadScene(menuSceneLoadParams, sceneLoadResults[menuSceneName]);
    var gameLoadTask = _sceneLoaderService.LoadScene(gameSceneLoadParams, sceneLoadResults[gameSceneName]);

    // Register sceneLoadProgress needed to track (can be used for loading bar)
    _progressTrackingService.RegisterLoadingProgress(scenesLoadProgress);

    // Await all scenes loading
    await UniTask.WhenAll(gameLoadTask, menuLoadTask);

    // Force the update of SetupProgress (can be use for object initialization, setup, assets loading ....) 
    foreach (var sceneLoadResult in sceneLoadResults)
        sceneLoadResult.Value.SceneLoadProgress.SetupProgress = 1;
}
```

---

# Notes:
If use this package in project for first time, it may rise some errors. This is caused because of large number of dependencies, including <b>Git dependencies</b>,
which are loaded via script on project open  (if these packages are not already in manifest file). Click <b>"Ignore"</b> and wait till unity will load everything.
<br><img src="https://i.postimg.cc/m2vRBf3g/Unity.png" alt="Unity Error" width="300">
<br><br>If the issue is not resolved because Fmod is not fully imported, remove old FMOD and restart project. This will force unity to load all dependencies again.