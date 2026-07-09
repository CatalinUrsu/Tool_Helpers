using UnityEngine;

namespace Helpers.Services
{
    /// <summary>
    /// Interface for managing camera-related operations in the application. <br />
    /// F.E., registering the main camera, managing multiple cameras, and retrieving cameras by name or tag.
    /// </summary>
    public interface IServiceCamera
    {
        void RegisterMainCamera(Camera camera);

        void RegisterCamera(string key, Camera camera, bool addToStack = true);
        
        void UnregisterCamera(Camera camera);
        
        Camera GetMainCamera();
        
        Camera GetCameraByKey(string name);

        Camera GetCameraByTag(string tag);
    }
}