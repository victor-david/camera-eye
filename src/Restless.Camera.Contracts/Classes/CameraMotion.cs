namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Provides an enumeration of possible camera motions that a <see cref="ICameraMotion"/> plugin can use.
    /// </summary>
    public enum CameraMotion
    {
        /// <summary>
        /// Stop the camera motion.
        /// </summary>
        Stop,
        /// <summary>
        /// Move the camera up.
        /// </summary>
        Up,
        /// <summary>
        /// Move the camera down.
        /// </summary>
        Down,
        /// <summary>
        /// Move the camera left.
        /// </summary>
        Left,
        /// <summary>
        /// Move the camera right.
        /// </summary>
        Right,
        /// <summary>
        /// Move the camera to center.
        /// </summary>
        Center,
        /// <summary>
        /// Move the camera up and left.
        /// </summary>
        UpLeft,
        /// <summary>
        /// Move the camera up and right.
        /// </summary>
        UpRight,
        /// <summary>
        /// Move the camera down and left.
        /// </summary>
        DownLeft,
        /// <summary>
        /// Move the camera down and right.
        /// </summary>
        DownRight,
        /// <summary>
        /// Start horizontal patrol.
        /// </summary>
        PatrolHorizontal,
        /// <summary>
        /// Start vertical patrol.
        /// </summary>
        PatrolVertical,
    }
}