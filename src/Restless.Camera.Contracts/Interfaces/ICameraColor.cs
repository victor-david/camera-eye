﻿using System;

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
        /// Initializes the camera values (brightenss, contrast, etc) by obtaining them from the camera.
        /// </summary>
        /// <param name="completed">A method to call when the values have been retrieved.</param>
        void InitializeCameraValues(Action completed);
    }
}