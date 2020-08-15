namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines methods that a camera plugin that supports presets must implement.
    /// </summary>
    public interface ICameraPreset
    {
        /// <summary>
        /// Gets the maximum number of supported presets.
        /// </summary>
        int MaxPreset { get; }

        /// <summary>
        /// Moves the camera to the specified preset position.
        /// </summary>
        /// <param name="preset">The preset number.</param>
        void MoveToPreset(int preset);

        /// <summary>
        /// Sets the specified preset.
        /// </summary>
        /// <param name="preset">The preset number.</param>
        void SetPreset(int preset);


        /// <summary>
        /// Clears the specified preset.
        /// </summary>
        /// <param name="preset">The preset number.</param>
        void ClearPreset(int preset);
    }
}