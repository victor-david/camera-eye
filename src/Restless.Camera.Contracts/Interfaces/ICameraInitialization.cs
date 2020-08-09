using System;

namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines a method that a plugin must implement if it supports <see cref="ICameraSettings"/> and/or <see cref="ICameraColor"/>
    /// </summary>
    public interface ICameraInitialization
    {
        /// <summary>
        /// Initializes the camera values (brightenss, contrast, etc) by obtaining them from the camera.
        /// </summary>
        /// <param name="completed">A method to call when the values have been retrieved.</param>
        void InitializeCameraValues(Action completed);
    }
}