namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines properties and methods that a camera plugin must implement
    /// if it can modify video settings such as brightness, contrast, flip and mirror.
    /// </summary>
    public interface ICameraSettings : ICameraInitialization
    {
        /// <summary>
        /// Gets a bitwise combination value that describes which setting items are supported.
        /// </summary>
        SettingItem Supported { get; }

        /// <summary>
        /// Gets or sets the brightness.
        /// </summary>
        int Brightness { get; set; }

        /// <summary>
        /// Gets or sets the contrast.
        /// </summary>
        int Contrast { get; set; }

        /// <summary>
        /// Gets or sets the hue.
        /// </summary>
        int Hue { get; set; }

        /// <summary>
        /// Gets or sets the saturation.
        /// </summary>
        int Saturation { get; set; }

        /// <summary>
        /// Sets whether or not the video is flipped.
        /// </summary>
        /// <param name="value">true to flip video; otherwise false.</param>
        void SetIsFlipped(bool value);

        /// <summary>
        /// Sets whether or not the video is mirrored.
        /// </summary>
        /// <param name="value">true to mirror video; otherwise false.</param>
        void SetIsMirrored(bool value);

        /// <summary>
        /// Sets the rotation of the video.
        /// </summary>
        /// <param name="value">The rotation.</param>
        void SetRotation(Rotation value);
    }
}