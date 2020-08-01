namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides application configuration services
    /// </summary>
    public class Config
    {
        #region Public fields
        /// <summary>
        /// Provides static default values for properties
        /// </summary>
        public static class Default
        {
            /// <summary>
            /// Provides static default values for the main window.
            /// </summary>
            public static class MainWindow
            {
                /// <summary>
                /// Gets the default width for the main window.
                /// </summary>
                public const int Width = 1260;

                /// <summary>
                /// Gets the default height for the main window.
                /// </summary>
                public const int Height = 680;

                /// <summary>
                /// Gets the minimum width for the main window.
                /// </summary>
                public const int MinWidth = 980;

                /// <summary>
                /// Gets the minimum height for the main window.
                /// </summary>
                public const int MinHeight = 590;
            }

            /// <summary>
            /// Provides static default values for the camera window.
            /// </summary>
            public static class CameraWindow
            {
                /// <summary>
                /// Gets the default width for the camera window.
                /// </summary>
                public const int Width = 800;

                /// <summary>
                /// Gets the default height for the camera window.
                /// </summary>
                public const int Height = 600;

                /// <summary>
                /// Gets the minimum width for the camera window.
                /// </summary>
                public const int MinWidth = 600;

                /// <summary>
                /// Gets the minimum height for the camera window.
                /// </summary>
                public const int MinHeight = 320;
            }

            public static class CameraConfigWindow
            {
                /// <summary>
                /// Gets the default width for the camera configuration window.
                /// </summary>
                public const int Width = 1080;

                /// <summary>
                /// Gets the default height for the camera configuration window.
                /// </summary>
                public const int Height = 600;
            }
        }
        #endregion
    }
}