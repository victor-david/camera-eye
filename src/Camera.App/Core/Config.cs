﻿using Restless.App.Database.Core;
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
            /// Provides static default values for the camera edit window.
            /// </summary>
            public static class CameraManageWindow
            {
                /// <summary>
                /// Gets the default width for the camera manage window.
                /// </summary>
                public const int Width = 916;

                /// <summary>
                /// Gets the default height for the camera manage window.
                /// </summary>
                public const int Height = 498;

                /// <summary>
                /// Gets the minimum width for the camera manage window.
                /// </summary>
                public const int MinWidth = 720;

                /// <summary>
                /// Gets the minimum height for the camera manage window.
                /// </summary>
                public const int MinHeight = 498;
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

        #region Window settings (Main)
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
        /// Gets or sets the left cooridinate of the main window.
        /// </summary>
        public int MainWindowLeft
        {
            get => GetItem(int.MaxValue);
            set => SetItem(value);
        }

        /// <summary>
        /// Gets or sets the top cooridinate of the main window.
        /// </summary>
        public int MainWindowTop
        {
            get => GetItem(int.MaxValue);
            set => SetItem(value);
        }

        /// <summary>
        /// Gets or sets a boolean value that determines if the window is topmost.
        /// </summary>
        public bool MainWindowTopmost
        {
            get => GetItem(false);
            set => SetItem(value);
        }

        /// <summary>
        /// Saves the main window for later restoral.
        /// </summary>
        /// <param name="window">The window.</param>
        public void SaveMainWindow(Window window)
        {
            if (window != null)
            {
                if (window.WindowState != WindowState.Maximized)
                {
                    MainWindowWidth = (int)window.Width;
                    MainWindowHeight = (int)window.Height;
                }

                /* don't save state as minimized. if closed as minmized, will restore as normal */
                MainWindowState = window.WindowState == WindowState.Minimized ? WindowState.Normal : window.WindowState;
                MainWindowLeft = (int)window.Left;
                MainWindowTop = (int)window.Top;
            }
        }

        /// <summary>
        /// Restores the main window to its saved size and position.
        /// </summary>
        /// <param name="window">The window.</param>
        public void RestoreMainWindow(Window window)
        {
            if (window != null)
            {
                window.MinWidth = Default.MainWindow.MinWidth;
                window.MinHeight = Default.MainWindow.MinHeight;
                window.WindowState = MainWindowState;
                window.Width = MainWindowWidth;
                window.Height = MainWindowHeight;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                /* default values are int.MaxValue which will only be present on first run. */
                if (MainWindowLeft != int.MaxValue && MainWindowTop != int.MaxValue)
                {
                    window.Left = MainWindowLeft;
                    window.Top = MainWindowTop;
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                }
            }
        }
        #endregion

        /************************************************************************/

        #region Window settings (Manage)
        /// <summary>
        /// Gets or sets the width of the camera manage window
        /// </summary>
        public int ManageWindowWidth
        {
            get => GetItem(Default.CameraManageWindow.Width);
            set => SetItem(value);
        }

        /// <summary>
        /// Gets or sets the height of the camera manage window
        /// </summary>
        public int ManageWindowHeight
        {
            get => GetItem(Default.CameraManageWindow.Height);
            set => SetItem(value);
        }

        /// <summary>
        /// Gets or sets the state of the camera manage window
        /// </summary>
        public WindowState ManageWindowState
        {
            get => (WindowState)GetItem((int)WindowState.Normal);
            set => SetItem((int)value);
        }

        /// <summary>
        /// Gets or sets the left cooridinate of the camera manage window.
        /// </summary>
        public int ManageWindowLeft
        {
            get => GetItem(int.MaxValue);
            set => SetItem(value);
        }

        /// <summary>
        /// Gets or sets the top cooridinate of the camera manage window.
        /// </summary>
        public int ManageWindowTop
        {
            get => GetItem(int.MaxValue);
            set => SetItem(value);
        }

        /// <summary>
        /// Saves the camera manage window for later restoral.
        /// </summary>
        /// <param name="window">The window.</param>
        public void SaveManageWindow(Window window)
        {
            if (window != null)
            {
                if (window.WindowState != WindowState.Maximized)
                {
                    ManageWindowWidth = (int)window.Width;
                    ManageWindowHeight = (int)window.Height;
                }

                /* don't save state as minimized. if closed as minmized, will restore as normal */
                ManageWindowState = window.WindowState == WindowState.Minimized ? WindowState.Normal : window.WindowState;
                ManageWindowLeft = (int)window.Left;
                ManageWindowTop = (int)window.Top;
            }
        }

        /// <summary>
        /// Restores the camera manage window to its saved size and position.
        /// </summary>
        /// <param name="window">The window.</param>
        public void RestoreManageWindow(Window window)
        {
            if (window != null)
            {
                window.MinWidth = Default.CameraManageWindow.MinWidth;
                window.MinHeight = Default.CameraManageWindow.MinHeight;
                window.WindowState = ManageWindowState;
                window.Width = ManageWindowWidth;
                window.Height = ManageWindowHeight;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                /* default values are int.MaxValue which will only be present on first run. */
                if (ManageWindowLeft != int.MaxValue && ManageWindowTop != int.MaxValue)
                {
                    window.Left = ManageWindowLeft;
                    window.Top = ManageWindowTop;
                    window.WindowStartupLocation = WindowStartupLocation.Manual;
                }
            }
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
    }
}