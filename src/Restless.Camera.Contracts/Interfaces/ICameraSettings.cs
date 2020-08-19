namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines properties and methods that a camera plugin must implement
    /// if it can modify video settings such as brightness, contrast, etc..
    /// </summary>
    public interface ICameraSettings : ICameraInitialization
    {
        /// <summary>
        /// Gets a bitwise combination value that describes which setting items are supported.
        /// </summary>
        CameraSetting Supported { get; }

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
    }
}