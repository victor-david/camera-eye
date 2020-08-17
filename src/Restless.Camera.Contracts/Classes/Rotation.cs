namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Provides an enumeration of rotation value when a plugin supports a rotation via <see cref="ICameraSettings"/>.
    /// </summary>
    public enum Rotation
    {
        /// <summary>
        /// Do not rotate the video.
        /// </summary>
        RotateZero,
        /// <summary>
        /// Rotate the video 90 degrees
        /// </summary>
        Rotate90,
        /// <summary>
        /// Rotate the video 270 degrees.
        /// </summary>
        Rotate270 = 128,
    }
}