﻿using Restless.App.Database.Tables;
using System.Windows;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides static methods for creating application windows.
    /// </summary>
    public static class WindowFactory
    {
        #region Main
        /// <summary>
        /// Provides static methods for creating the main application window.
        /// </summary>
        public static class Main
        {
            /// <summary>
            /// Creates an instance of MainWindow and its corresponding view model.
            /// </summary>
            /// <param name="viewModel">The view model to inject, or null to use the default.</param>
            /// <returns>The window</returns>
            public static MainWindow Create(ApplicationViewModel viewModel = null)
            {
                var window = new MainWindow()
                {
                    DataContext = viewModel ?? new MainWindowViewModel()
                };

                Config.Instance.RestoreMainWindow(window);

                (window.DataContext as WindowViewModel)?.SetWindowOwner(window);
                TextOptions.SetTextFormattingMode(window);
                return window;
            }
        }
        #endregion

        /************************************************************************/

        #region Camera
        /// <summary>
        /// Provides static methods for creating camera windows.
        /// </summary>
        public static class Camera
        {
            /// <summary>
            /// Creates an instance of CameraWindow and its corresponding view model.
            /// </summary>
            /// <param name="viewModel">The view model to inject, or null to use the default.</param>
            /// <returns>The window</returns>
            public static CameraWindow Create(CameraRow camera, ApplicationViewModel viewModel = null)
            {
                var window = new CameraWindow()
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Width = Config.Default.CameraWindow.Width,
                    Height = Config.Default.CameraWindow.Height,
                    MinWidth = Config.Default.CameraWindow.MinWidth,
                    MinHeight = Config.Default.CameraWindow.MinHeight,
                    DataContext = viewModel ?? new CameraWindowViewModel(camera)
                };

                (window.DataContext as WindowViewModel)?.SetWindowOwner(window);

                TextOptions.SetTextFormattingMode(window);
                return window;
            }
        }
        #endregion

        /************************************************************************/

        #region CameraConfig
        /// <summary>
        /// Provides static methods for creating camera configuration windows.
        /// </summary>
        public static class CameraEdit
        {

            public static CameraEditWindow Create(CameraRow camera)
            {
                var window = new CameraEditWindow()
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    DataContext = new CameraEditViewModel(camera),
                    Width = Config.Default.CameraEditWindow.Width,
                    Height = Config.Default.CameraEditWindow.Height,
                    MinWidth = Config.Default.CameraEditWindow.MinWidth,
                    MinHeight = Config.Default.CameraEditWindow.MinHeight,
                };

                (window.DataContext as WindowViewModel)?.SetWindowOwner(window);

                TextOptions.SetTextFormattingMode(window);
                return window;
            }
        }
        #endregion

        /************************************************************************/

        #region TextOptions (private)
        private static class TextOptions
        {
            public static void SetTextFormattingMode(DependencyObject element)
            {
                System.Windows.Media.TextOptions.SetTextFormattingMode(element, System.Windows.Media.TextFormattingMode.Display);
            }
        }
        #endregion
    }
}
