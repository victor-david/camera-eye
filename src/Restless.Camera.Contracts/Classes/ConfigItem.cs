namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Provides a helper enumeration of configuration ietms that a plugin may use to map operations to commands.
    /// </summary>
    public enum ConfigItem
    {
        /// <summary>
        /// Brightness.
        /// </summary>
        Brightness, 
        /// <summary>
        /// Constrast.
        /// </summary>
        Contrast,
        /// <summary>
        /// Hue.
        /// </summary>
        Hue,
        /// <summary>
        /// Saturation.
        /// </summary>
        Saturation,
        /// <summary>
        /// Flip the video.
        /// </summary>
        FlipOn,
        /// <summary>
        /// Unflip the video (normal)
        /// </summary>
        FlipOff,
        /// <summary>
        /// Mirror the video.
        /// </summary>
        MirrorOn, 
        /// <summary>
        /// Unmirror the video (normal)
        /// </summary>
        MirrorOff,
    }
}
