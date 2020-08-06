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
        /// Low frame rate
        /// </summary>
        FrameRateLow = 128,
        /// <summary>
        /// Low frame rate
        /// </summary>
        FrameRateMedium = 256,
        /// <summary>
        /// High frame rate
        /// </summary>
        FrameRateHigh = 512,

    }
}
