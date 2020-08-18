using System.Threading.Tasks;

namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines methods that a camera plugin that supports camera motion must implement.
    /// </summary>
    public interface ICameraMotion
    {
        /// <summary>
        /// Gets the speed for camera motion, a value between 0-100 inclusive.
        /// When setting motion speed, implementors need to accept 0-100 and adjust for the particulars of the camera accordingly.
        /// </summary>
        int MotionSpeed { get; }

        /// <summary>
        /// Sets the motion speed.
        /// Implementors need to accept 0-100 and adjust for the particulars of the camera accordingly.
        /// </summary>
        /// <param name="value">The value (0-100)</param>
        /// <returns>true if speed set successfully; otherwise, false.</returns>
        Task<bool> SetMotionSpeedAsync(int value);

        /// <summary>
        /// Moves the camera as specified by <paramref name="motion"/>.
        /// </summary>
        /// <param name="motion">The motion.</param>
        void Move(CameraMotion motion);
    }
}