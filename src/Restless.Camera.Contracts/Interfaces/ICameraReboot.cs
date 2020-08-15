namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines properties and methods that a camera plugin must implement
    /// if it can perform a reboot operation.
    /// </summary>
    public interface ICameraReboot
    {
        /// <summary>
        /// Reboots the camera.
        /// </summary>
        void Reboot();
    }
}