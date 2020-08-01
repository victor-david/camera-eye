namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Provides an enumeration of frame rates that may be applied to a video stream.
    /// It is up to the implementation to decide how these values are interpreted.
    /// </summary>
    public enum FrameRate
    {
        /// <summary>
        /// Low frame rate.
        /// </summary>
        Low,
        /// <summary>
        /// Medium frame rate.
        /// </summary>
        Medium,
        /// <summary>
        /// High frame rate.
        /// </summary>
        High,
    }
}