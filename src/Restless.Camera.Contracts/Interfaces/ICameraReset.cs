namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines properties and methods that a camera plugin must implement
    /// if it can perform reset and reboot operatopns.
    /// </summary>
    public interface ICameraReset
    {
        /// <summary>
        /// Resets the camera settings to defaults.
        /// </summary>
        void Reset();

        /// <summary>
        /// Reboots the camera.
        /// </summary>
        void Reboot();
    }
}