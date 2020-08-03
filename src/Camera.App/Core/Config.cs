using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using System.Windows;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides application configuration services
    /// </summary>
    public class Config : Tools.Database.SQLite.KeyValueTableBase
    {
        #region Private
        #endregion

        /************************************************************************/

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
                public const int MinWidth = 343;

                /// <summary>
                /// Gets the minimum height for the main window.
                /// </summary>
                public const int MinHeight = 418;
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

        /************************************************************************/

        #region Static singleton access and constructor
        /// <summary>
        /// Gets the singleton instance of this class
        /// </summary>
        public static Config Instance { get; } = new Config();

        private Config() : base(DatabaseController.Instance.GetTable<ConfigTable>())
        {
        }

        /// <summary>
        /// Static constructor. Tells C# compiler not to mark type as beforefieldinit.
        /// </summary>
        static Config()
        {
        }
        #endregion

        /************************************************************************/

        #region Window settings
        /// <summary>
        /// Gets or sets the width of the main window
        /// </summary>
        public int MainWindowWidth
        {
            get => GetItem(Default.MainWindow.Width);
            set => SetItem(value);
        }

        /// <summary>
        /// Gets or sets the height of the main window
        /// </summary>
        public int MainWindowHeight
        {
            get => GetItem(Default.MainWindow.Height);
            set => SetItem(value);
        }

        /// <summary>
        /// Gets or sets the state of the main window
        /// </summary>
        public WindowState MainWindowState
        {
            get => (WindowState)GetItem((int)WindowState.Normal);
            set => SetItem((int)value);
        }

        /// <summary>
        /// Gets or sets a boolean value that determines if the window is topmost.
        /// </summary>
        public bool MainWindowTopmost
        {
            get => GetItem(false);
            set => SetItem(value);
        }
        #endregion

        /************************************************************************/

        #region Cameras
        /// <summary>
        /// Gets or sets whether the camera list is visible.
        /// </summary>
        public bool IsCameraListVisible
        {
            get => GetItem(false);
            set => SetItem(value);
        }
        /// <summary>
        /// Gets or sets the camera wall grid layout.
        /// </summary>
        public WallGridLayout GridLayout
        {
            get => (WallGridLayout)GetItem((int)WallGridLayout.TwoByTwo);
            set => SetItem((int)value);
        }
        #endregion
    }
}