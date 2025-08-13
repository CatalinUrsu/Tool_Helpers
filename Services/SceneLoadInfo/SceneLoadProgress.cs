namespace Helpers.Services
{
    /// <summary>
    /// Represents the progress of scene loading, including the scene itself and its EntryPoint
    /// </summary>
    public class SceneLoadProgress
    {
        /// <summary>
        /// The progress of the scene loading, represented as a float value between 0 and 1.
        /// </summary>
        public float SceneProgress;

        /// <summary>
        /// The progress of the entry point loading, represented as a float value between 0 and 1.
        /// </summary>
        public float EntryPointProgress;

        /// <summary>
        /// Calculates the average progress of the scene and entry point loading.
        /// </summary>
        public float GetAvgProgress() => (SceneProgress + EntryPointProgress) / 2;
    }
}