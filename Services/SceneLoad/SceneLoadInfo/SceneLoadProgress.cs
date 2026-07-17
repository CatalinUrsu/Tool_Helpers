namespace Helpers.Services
{
    /// <summary>
    /// Progress of scene loading and setup-ing
    /// </summary>
    public class SceneLoadProgress
    {
        /// <summary>
        /// The progress of the scene loading, (value between 0 and 1)
        /// </summary>
        public float LoadProgress;

        /// <summary>
        /// The progress of elements initialization like (object spawning, setup, loading additional content
        /// </summary>
        public float SetupProgress;

        /// <summary>
        /// Calculates the average progress of the scene load and scene setup.
        /// </summary>
        public float GetAvgProgress() => (LoadProgress + SetupProgress) / 2;
    }
}