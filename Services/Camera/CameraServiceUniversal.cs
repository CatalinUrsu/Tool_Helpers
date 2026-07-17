using UnityEngine;

#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

namespace Helpers.Services
{
public class CameraServiceUniversal : CameraService
{
#if UNITY_RENDER_PIPELINE_UNIVERSAL

    UniversalAdditionalCameraData _cameraData;

    public override void RegisterMainCamera(Camera camera)
    {
        base.RegisterMainCamera(camera);

        _cameraData = camera.GetUniversalAdditionalCameraData();
    }

    public override void RegisterCamera(string key, Camera camera, bool addToStack = false)
    {
        base.RegisterCamera(key, camera, addToStack);

        if (addToStack && !_cameraData.cameraStack.Contains(camera))
            _cameraData.cameraStack.Add(camera);
    }

    public override void UnregisterCamera(string key)
    {
        var cameraExist = _cachedCameras.TryGetValue(key, out var camera);

        base.UnregisterCamera(key);

        if (cameraExist && camera != _mainCamera)
            _cameraData.cameraStack.Remove(camera);
    }
#endif
}
}