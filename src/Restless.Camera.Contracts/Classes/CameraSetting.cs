using System;

namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Provides an enumeration of items that may be supported via <see cref="ICameraSettings"/>.
    /// </summary>
    [Flags]
    public enum CameraSetting
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Brightness.
        /// </summary>
        Brightness = 1,
        /// <summary>
        /// Constrast.
        /// </summary>
        Contrast = 2,
        /// <summary>
        /// Hue.
        /// </summary>
        Hue = 4,
        /// <summary>
        /// Saturation.
        /// </summary>
        Saturation = 8,
        /// <summary>
        /// Flip the video.
        /// </summary>
        Flip = 16,
        /// <summary>
        /// Mirror the video.
        /// </summary>
        Mirror = 32,
        /// <summary>
        /// Rotate the video according to the <see cref="Rotation"/> enumeration.
        /// A plugin that supports rotation does not need to support all possible rotation values,
        /// and should ignore those it can't do.
        /// </summary>
        Rotation = 64,
    }
}