using System;

namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines properties and methods that a camera plugin must implement
    /// if it can modify video color parameters such as brightness and contrast
    /// </summary>
    public interface ICameraColor
    {
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
        /// Occurs when the color values (brightness, contrast, etc.) have been retrieved from the camera.
        /// </summary>
        event EventHandler ColorValuesInitialized;

        ///// <summary>
        ///// Gets the current brightness value.
        ///// </summary>
        ///// <returns>The value</returns>
        //int GetBrightness();

        ///// <summary>
        ///// Gets the current contrast value.
        ///// </summary>
        ///// <returns>The value</returns>
        //int GetContrast();

        ///// <summary>
        ///// Gets the current hue value.
        ///// </summary>
        ///// <returns>The value</returns>
        //int GetHue();

        ///// <summary>
        ///// Gets the current saturation value.
        ///// </summary>
        ///// <returns>The value</returns>
        //int GetSaturation();

        ///// <summary>
        ///// Sets the brightness.
        ///// </summary>
        ///// <param name="value">The value to set.</param>
        //void SetBrightness(int value);

        ///// <summary>
        ///// Sets the contrast.
        ///// </summary>
        ///// <param name="value">The value to set.</param>
        //void SetContrast(int value);

        ///// <summary>
        ///// Sets the hue.
        ///// </summary>
        ///// <param name="value">The value to set.</param>
        //void SetHue(int value);

        ///// <summary>
        ///// Sets the saturation.
        ///// </summary>
        ///// <param name="value">The value to set.</param>
        //void SetSaturation(int value);

        ///// <summary>
        ///// Occurs when the color values (brightness, contrast, etc.) have been retrieved from the camera.
        ///// </summary>
        //event EventHandler ColorValuesInitialized;
    }
}
