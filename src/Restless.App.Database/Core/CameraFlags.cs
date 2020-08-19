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
        /// Camera is on the wall
        /// </summary>
        IncludeOnWall = 8,
        /// <summary>
        /// Translate X movement (left / right)
        /// </summary>
        TranslateX = 16,
        /// <summary>
        /// Translate Y movement (up / down)
        /// </summary>
        TranslateY = 32,
        /// <summary>
        /// Flip the video image.
        /// </summary>
        Flip = 64,
        /// <summary>
        /// Mirror the video image.
        /// </summary>
        Mirror = 128,
    }
}
