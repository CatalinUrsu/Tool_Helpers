using UnityEngine;

namespace Helpers.Services
{
public class CameraServiceURP : CameraService
{
    public override void UnregisterCamera(string key)
    {
        var cameraExist = _cameras.TryGetValue(key, out var camera);

        base.UnregisterCamera(key);

        if (cameraExist)
            RemoveCameraFromAnyStack(camera);
    }

    public override void AddToStack(Camera sourceCamera)
    {
        var cameraUrpData = sourceCamera.GetUrpCameraData();

        if (!cameraUrpData.cameraStack.Contains(sourceCamera))
            cameraUrpData.cameraStack.Add(sourceCamera);
    }

    void RemoveCameraFromAnyStack(Camera camera)
    {
        foreach (var registeredCamera in _cameras.Values)
            registeredCamera.GetUrpCameraData().cameraStack.Remove(camera);
    }
}
}
