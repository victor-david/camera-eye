﻿namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides an enumeration of the command types that may be pushed.
    /// </summary>
    public enum PushCommandType
    {
        /// <summary>
        /// No command.
        /// </summary>
        None,
        /// <summary>
        /// Remove camera from wall.
        /// </summary>
        RemoveFromWall,
        /// <summary>
        /// Update camera's status banner.
        /// </summary>
        UpdateStatusBanner,
        /// <summary>
        /// Show the camera's location on the wall.
        /// </summary>
        ShowCameraLocation,
    }
}