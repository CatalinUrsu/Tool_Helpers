using UnityEngine;
using System.Collections.Generic;

namespace Helpers.Services
{
public class CameraService : ICameraService
{
    protected readonly Dictionary<string, Camera> _cameras = new();

    public virtual void RegisterCamera(string key, Camera camera)
    {
        if (!_cameras.TryAdd(key, camera))
            Debug.LogError($"[CamerasService] Camera with key '{key}' already exist");
    }

    public virtual void UnregisterCamera(string key) => _cameras.Remove(key);

    public Camera GetCamera(string name)
    {
        if (_cameras.TryGetValue(name, out var camera))
            return camera;

        Debug.LogError($"[CamerasService] Not found camera with key '{name}'!");
        return null;
    }

    public virtual void AddToStack(Camera sourceCamera) { }
}
}
