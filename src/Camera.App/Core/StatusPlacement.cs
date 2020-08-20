namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides a enumeration that describes where the video status banner of a <see cref="CameraControl"/> is placed.
    /// </summary>
    public enum StatusPlacement
    {
        /// <summary>
        /// Status is hidden.
        /// </summary>
        None,
        /// <summary>
        /// Status is placed at the top of the video.
        /// </summary>
        Top,
        /// <summary>
        /// Status is placed at the bottom of the video.
        /// </summary>
        Bottom,
    }
}
