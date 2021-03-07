using System;

namespace Restless.App.Database.Core
{
    /// <summary>
    /// Provides an enumeration for values used for camera flags
    /// </summary>
    [Flags]
    public enum CameraFlags : long
    {
        /// <summary>
        /// No flags are set.
        /// </summary>
        None = 0,
        /// <summary>
        /// Status is displayed top when visible.
        /// </summary>
        StatusTop = 1,
        /// <summary>
        /// Status is displayed bottom when visible.
        /// </summary>
        StatusBottom = 2,
        /// <summary>
        /// When status is displayed, include the camera name.
        /// </summary>
        StatusCameraName = 4,
        /// <summary>
        /// When status is displayed, include the current date/time.
        /// </summary>
        StatusDateTime = 8,
        /// <summary>
        /// When status is displayed, include the frame count.
        /// </summary>
        StatusFrameCount = 16,
        /// <summary>
        /// Camera is on the wall
        /// </summary>
        IncludeOnWall = 32,
        /// <summary>
        /// Translate X movement (left / right)
        /// </summary>
        TranslateX = 64,
        /// <summary>
        /// Translate Y movement (up / down)
        /// </summary>
        TranslateY = 128,
        /// <summary>
        /// Flip the video image.
        /// </summary>
        Flip = 256,
        /// <summary>
        /// Mirror the video image.
        /// </summary>
        Mirror = 512,
        /// <summary>
        /// Pan and tilt the camera by using the mouse.
        /// </summary>
        MouseMotion = 1024,
    }
}
