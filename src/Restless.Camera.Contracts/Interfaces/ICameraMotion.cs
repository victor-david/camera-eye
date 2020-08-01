namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines methods that a camera plugin that supports camera motion must implement.
    /// </summary>
    public interface ICameraMotion
    {
        /// <summary>
        /// Gets the minimum supported motion speed.
        /// </summary>
        int MinSpeed { get; }

        /// <summary>
        /// Gets the maximum supported motion speed.
        /// </summary>
        int MaxSpeed { get; }

        /// <summary>
        /// Gets or sets the speed for camera motion.
        /// </summary>
        int MotionSpeed { get; set; }

        /// <summary>
        /// Moves the camera as specified by <paramref name="motion"/>.
        /// </summary>
        /// <param name="motion">The motion.</param>
        void Move(CameraMotion motion);
    }
}