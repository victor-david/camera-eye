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
    }
}