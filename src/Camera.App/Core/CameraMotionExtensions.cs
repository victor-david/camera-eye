using Restless.Camera.Contracts;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="CameraMotion"/>
    /// </summary>
    public static class CameraMotionExtensions
    {
        /// <summary>
        /// Gets a boolean value that indicates if motion is on the X axis
        /// </summary>
        /// <param name="motion">The motion.</param>
        /// <returns>true if X axis motion (left / right); otherwise, false.</returns>
        public static bool IsX(this CameraMotion motion)
        {
            return motion == CameraMotion.Left || motion == CameraMotion.Right;
        }

        /// <summary>
        /// Gets a boolean value that indicates if motion is on the Y axis
        /// </summary>
        /// <param name="motion">The motion.</param>
        /// <returns>true if Y axis motion (up / down); otherwise, false.</returns>
        public static bool IsY(this CameraMotion motion)
        {
            return motion == CameraMotion.Up || motion == CameraMotion.Down;
        }

        /// <summary>
        /// Gets the translated motion for the X axis.
        /// </summary>
        /// <param name="motion">The motion.</param>
        /// <returns>The translated motion.</returns>
        public static CameraMotion TranslatedX(this CameraMotion motion)
        {
            return motion switch
            {
                CameraMotion.Left => CameraMotion.Right,
                CameraMotion.Right => CameraMotion.Left,
                _ => motion,
            };
        }

        /// <summary>
        /// Gets the translated motion for the Y axis.
        /// </summary>
        /// <param name="motion">The motion.</param>
        /// <returns>The translated motion.</returns>
        public static CameraMotion TranslatedY(this CameraMotion motion)
        {
            return motion switch
            {
                CameraMotion.Up => CameraMotion.Down,
                CameraMotion.Down => CameraMotion.Up,
                _ => motion,
            };
        }
    }
}