using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Helpers.Services
{
public class CameraService : IServiceCamera
{
    Camera _mainCamera;
    readonly Dictionary<string, Camera> _cachedCameras = new();

    public virtual void RegisterMainCamera(Camera camera)
    {
        if (_mainCamera != null) return;

        _mainCamera = camera;
    }

    public virtual void RegisterCamera(string key, Camera camera, bool addToStack = true)
    {
        if (_cachedCameras.TryAdd(key, camera)) return;
            Debug.LogError($"[CamerasService] Camera with key '{key}' already exist");
        
    }

    public virtual void UnregisterCamera(Camera camera)
    {
        if (_cachedCameras.ContainsKey(camera.name))
            _cachedCameras.Remove(camera.name);
    }

    public Camera GetMainCamera() => _mainCamera;

    public Camera GetCameraByKey(string name)
    {
        if (_cachedCameras.TryGetValue(name, out var camera))
            return camera;

        Debug.LogError($"[CamerasService] Not found camera with name '{name}'!");
        return null;
    }

    public Camera GetCameraByTag(string tag)
    {
        var camera = _cachedCameras.FirstOrDefault(x => x.Value.CompareTag(tag)).Value;
        if (camera != null)
            return camera;

        Debug.LogError($"[CamerasService] Not found camera with tag '{tag}'!");
        return null;
    }

}
}