<h1 align="center">
Helpers
</h1>
<div align="center">
Additional helpful scripts for unity, that are designed to be used in multiple projects
</div>

# Contents:
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


# Audio 🔊
For audio maangement, is used <b>FMOD</b> - sound engine that cover all needs, this is first project where I used it and now it will be a permanent component
of all my projects.
<br><img src="https://i.postimg.cc/L6TnZtFd/Fmod-Events.png" alt="FmodEvents" width="500">

It’s great tool for memory management for audio. Instead of working with audio self, need to create events from audio clips.
These events need to be stored in banks that can be loaded and unloaded at runtime. Also, it has intuitive and powerful profiling window, personally I used
it to check the optimal amount of event instances in scene, like - shoot or hit audio.
<br><img src="https://i.postimg.cc/FHCzThKD/Fmod-Pooling.png" width="500">

For controlling volume and effect during runtime, I used VCA and Snapshots that also are extremely easy to set and change in unity. And last but not
least - Audio Optimization, all settings that can be changed in Unity (EncodingFormat, LoadingMode, SampleRate) are done in FMOD.
<br><img src="https://i.postimg.cc/FHH1bYhY/Fmod-Optimization.png" width="250">


# <br>Pool System 🔄
For memory and cpu optimization, it’s a good practice to use pooling system instead of creating and destroy objects each time.
Unity has its own PoolSystem, but I wanted to make my own, because I have more control of it, can expand it and so on. My PoolSystem has 3
components : [Pool](#pool-), [PooledObject](#pooled-object-), [Factory](#factory-).
<br><img src="https://i.postimg.cc/mrcDqKzR/Pool.png" alt="Pool" width="370">

## Pool ♻️
Pool is just a generic pool class, used only for get and release objects, similar to unity’s one, but simplified

## Pooled Object 📦
Base class for objects that are pooled. Has the option to set some config when it is instantiated and when it gets from pool (ex: set initial speed,
set different texture each time when it gets from pool). And also has option to release itself to pool (ex: when bullet hit something)
```csharp
using System;
using UnityEngine;

namespace Helpers.PoolSystem
{
public class PooledObject : MonoBehaviour
{
    public event Action OnReleaseToPool;

    public virtual void Init(Action<PooledObject> onReleaseToPool, object config) => OnReleaseToPool += () => onReleaseToPool(this);

    public virtual void Set(object config = null) { }

    protected virtual void OnReleaseToPool_raise() => OnReleaseToPool?.Invoke();
}
}
```


## Factory 🏭
Because creation of objects using pool was similar for each pooled object (bullet, effects, enemies) I decided to use a factory pattern for it.
I have two factories that can be used in any projects <b>([FactoryGO](PoolSystem/FactoryGO.cs), [FactoryFmodEvents](PoolSystem/FactoryFmodEvents.cs))</b>. Both of them are used just to set pools and both of them
use a Builder from more friendly LINQ-style code and as a bonus it resolves some “issues” like - the need to have multiple constructors, telescoping
constructors, using optional parameters.
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

# Notes:
If use this package in project for first time, it may rise some errors. This is caused because of large number of dependencies, including <b>Git dependencies</b>,
which are loaded via script on project open  (if these packages are not already in manifest file). Click <b>"Ignore"</b> and wait till unity will load everything.
<br><img src="https://i.postimg.cc/m2vRBf3g/Unity.png" alt="Unity Error" width="300">
<br><br>If the issue is not resolved because Fmod is not fully imported, remove old FMOD and restart project. This will force unity to load all dependencies again.