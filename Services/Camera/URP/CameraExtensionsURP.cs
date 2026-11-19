using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Helpers.Services
{
public static class CameraExtensionsURP
{
    public static UniversalAdditionalCameraData GetUrpCameraData(this Camera camera)
    {
        var universalCameraData = camera.GetUniversalAdditionalCameraData();

        if (universalCameraData == null)
            Debug.LogError($"[CamerasService] Camera '{camera.name}' does not have UniversalAdditionalCameraData component!");

        return universalCameraData;
    }
}
}
