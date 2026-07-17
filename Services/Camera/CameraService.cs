using UnityEngine;
using System.Collections.Generic;

namespace Helpers.Services
{
public class CameraService : ICameraService
{
    protected Camera _mainCamera;
    protected readonly Dictionary<string, Camera> _cachedCameras = new();

    public virtual void RegisterMainCamera(Camera camera)
    {
        if (_mainCamera != null) return;

        _mainCamera = camera;
    }

    public Camera GetMainCamera() => _mainCamera;

    public virtual void RegisterCamera(string key, Camera camera, bool addToStack = false)
    {
        if (!_cachedCameras.TryAdd(key, camera))
            Debug.LogError($"[CamerasService] Camera with key '{key}' already exist");
    }

    public virtual void UnregisterCamera(string key) => _cachedCameras.Remove(key);

    public Camera GetCameraByKey(string name)
    {
        if (_cachedCameras.TryGetValue(name, out var camera))
            return camera;

        Debug.LogError($"[CamerasService] Not found camera with key '{name}'!");
        return null;
    }
}
}