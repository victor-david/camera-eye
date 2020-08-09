namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines methods that a camera plugin that supports camera motion must implement.
    /// </summary>
    public interface ICameraMotion
    {
        /// <summary>
        /// Gets or sets the speed for camera motion.
        /// Implementors need to accept 0-100 and adjust for the particulars of the camera accordingly.
        /// </summary>
        int MotionSpeed { get; set; }

        /// <summary>
        /// Moves the camera as specified by <paramref name="motion"/>.
        /// </summary>
        /// <param name="motion">The motion.</param>
        void Move(CameraMotion motion);
    }
}