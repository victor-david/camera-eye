using System;

namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Specifies the orientation of the display.
    /// </summary>
    [Flags]
    public enum Orientation
    {
        /// <summary>
        /// Normal.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Display is flipped vertically.
        /// </summary>
        Flip = 1,
        /// <summary>
        /// Display is mirrored horizontally.
        /// </summary>
        Mirror = 2,
        /// <summary>
        /// Display is both flipped and mirrored
        /// </summary>
        FlipAndMirror = 3,
    }
}