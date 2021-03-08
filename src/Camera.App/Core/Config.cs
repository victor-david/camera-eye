using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using System.Collections.Generic;
using System.Windows;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides application configuration services
    /// </summary>
    public class Config : Tools.Database.SQLite.KeyValueTableBase
    {
        #region Private
        private readonly Dictionary<WindowKey, WindowConfig> windowConfig;
        #endregion

        /************************************************************************/

        #region WindowKey (public)
        /// <summary>
        /// Identifies the window to be saved and restored.
        /// </summary>
        public enum WindowKey
        {
            /// <summary>
            /// The main application window.
            /// </summary>
            Main,
            /// <summary>
            /// The window that manages a camera.
            /// </summary>
            CameraManage,
            /// <summary>
            /// The application settings window
            /// </summary>
            AppSettings
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
            windowConfig = new Dictionary<WindowKey, WindowConfig>()
            { 
                { WindowKey.Main, new WindowConfig(propertyPreface: "Main", defaultWidth: 1260, defaultHeight: 680, minWidth: 343, minHeight: 418) },
                { WindowKey.CameraManage, new WindowConfig(propertyPreface: "Manage", defaultWidth: 920, defaultHeight: 580, minWidth: 920, minHeight: 580) },
                { WindowKey.AppSettings, new WindowConfig(propertyPreface: "AppSettings", defaultWidth: 536, defaultHeight: 370, minWidth: 536, minHeight: 370) }
            };
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
        /// Saves the state of the specified window.
        /// </summary>
        /// <param name="key">The window key that identifies the configuration settings.</param>
        /// <param name="window">The window instance being saved.</param>
        public void SaveWindow(WindowKey key, Window window)
        {
            if (window != null && windowConfig.TryGetValue(key, out WindowConfig config))
            {
                if (window.WindowState != WindowState.Maximized)
                {
                    SetItem((int)window.Width, config.GetPropertyId(WindowConfig.Property.Width));
                    SetItem((int)window.Height, config.GetPropertyId(WindowConfig.Property.Height));
                }

                /* don't save state as minimized. if closed as minmized, will restore as normal */
                int state = (int)(window.WindowState == WindowState.Minimized ? WindowState.Normal : window.WindowState);
                SetItem(state, config.GetPropertyId(WindowConfig.Property.State));
                SetItem((int)window.Left, config.GetPropertyId(WindowConfig.Property.Left));
                SetItem((int)window.Top, config.GetPropertyId(WindowConfig.Property.Top));
            }
        }

        /// <summary>
        /// Restores the state of the specified window.
        /// </summary>
        /// <param name="key">The window key that identifies the configuration settings</param>
        /// <param name="window">The window instance being restored.</param>
        public void RestoreWindow(WindowKey key, Window window)
        {
            if (window != null && windowConfig.TryGetValue(key, out WindowConfig config))
            {
                window.MinWidth = windowConfig[key].MinWidth;
                window.MinHeight = windowConfig[key].MinHeight;
                window.WindowState = (WindowState)GetItem((int)WindowState.Normal, config.GetPropertyId(WindowConfig.Property.State));
                window.Width = GetItem(windowConfig[key].DefaultWidth, config.GetPropertyId(WindowConfig.Property.Width));
                window.Height = GetItem(windowConfig[key].DefaultHeight, config.GetPropertyId(WindowConfig.Property.Height));
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                /* default values are int.MaxValue which will only be present on first run. */
                int left = GetItem(int.MaxValue, config.GetPropertyId(WindowConfig.Property.Left));
                int top = GetItem(int.MaxValue, config.GetPropertyId(WindowConfig.Property.Top));
                if (left != int.MaxValue && top != int.MaxValue)
                {
                    window.Left = left;
                    window.Top = top;
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                }
            }
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
            get => GetItem(true);
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

        /************************************************************************/

        #region Other
        /// <summary>
        /// Gets or sets whether proxy detection is disabled.
        /// </summary>
        public bool IsProxyDetectionDisabled
        {
            get => GetItem(false);
            set => SetItem(value);
        }
        #endregion

        /************************************************************************/

        #region Private helper class
        /// <summary>
        /// Represents configuration values for a single window
        /// </summary>
        private class WindowConfig
        {
            private readonly string preface;
            public int DefaultWidth { get; }
            public int DefaultHeight { get; }
            public int MinWidth { get; }
            public int MinHeight { get; }

            /// <summary>
            /// Specifies parameters for <see cref="GetPropertyId(Property)"/>
            /// </summary>
            public enum Property
            {
                State,
                Width,
                Height,
                Left,
                Top,
            }

            public WindowConfig(string propertyPreface, int defaultWidth, int defaultHeight, int minWidth, int minHeight)
            {
                preface = propertyPreface;
                DefaultWidth = defaultWidth;
                DefaultHeight = defaultHeight;
                MinWidth = minWidth;
                MinHeight = minHeight;
            }

            /// <summary>
            /// Provides a consistent means of getting configuration property ids
            /// </summary>
            /// <param name="prop">The property</param>
            /// <returns>An id based on the preface and the specified property</returns>
            public string GetPropertyId(Property prop)
            {
                return $"{preface}Window{prop}";
            }
        }
        #endregion
    }
}