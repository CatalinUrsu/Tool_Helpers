using UnityEngine;

namespace Helpers.Services
{
    /// <summary>
    /// Service for managing camera-related operations in the application. <br />
    /// F.E., registering the main camera, managing multiple cameras, and retrieving cameras by name or tag.
    /// </summary>
    public interface ICameraService
    {
        void RegisterMainCamera(Camera camera);

        Camera GetMainCamera();
        
        void RegisterCamera(string key, Camera camera, bool addToStack = true);

        void UnregisterCamera(string key);

        Camera GetCameraByKey(string name);
    }
}