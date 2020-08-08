namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines properties and methods that a camera plugin must implement
    /// if it can modify video settings such as flip and mirror.
    /// </summary>
    public interface ICameraSettings
    {
        /// <summary>
        /// Sets video flip
        /// </summary>
        /// <param name="value">true to flip video; otherwise, false.</param>
        void SetFlip(bool value);

        /// <summary>
        /// Sets video mirror.
        /// </summary>
        /// <param name="value">true to mirror video; otherwise false.</param>
        void SetMirror(bool value);

        /// <summary>
        /// Sets infra red.
        /// </summary>
        /// <param name="value">true to turn on infra red; false to turn it off.</param>
        void SetInfraRed(bool value);
    }
}