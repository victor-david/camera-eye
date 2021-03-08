using Restless.App.Database.Tables;
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

                Config.Instance.RestoreWindow(Config.WindowKey.Main, window);

                (window.DataContext as WindowViewModel)?.SetWindowOwner(window);
                TextOptions.SetTextFormattingMode(window);
                return window;
            }
        }
        #endregion

        /************************************************************************/

        #region CameraManage
        /// <summary>
        /// Provides static methods for creating camera configuration windows.
        /// </summary>
        public static class CameraManage
        {
            /// <summary>
            /// Creates an instance of CameraManageWindow and its corresponding view model.
            /// </summary>
            /// <param name="camera">The camera to manage.</param>
            /// <returns>The window.</returns>
            public static CameraManageWindow Create(CameraRow camera)
            {
                var window = new CameraManageWindow()
                {
                    DataContext = new CameraManageViewModel(camera),
                };

                Config.Instance.RestoreWindow(Config.WindowKey.CameraManage, window);

                (window.DataContext as WindowViewModel)?.SetWindowOwner(window);
                TextOptions.SetTextFormattingMode(window);
                return window;
            }
        }
        #endregion

        /************************************************************************/

        #region AppSettings
        /// <summary>
        /// Provides static methods for creating an application settings window.
        /// </summary>
        public static class AppSettings
        {
            public static AppSettingsWindow Create()
            {
                var window = new AppSettingsWindow()
                {
                    DataContext = new AppSettingsViewModel()
                };

                Config.Instance.RestoreWindow(Config.WindowKey.AppSettings, window);

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
