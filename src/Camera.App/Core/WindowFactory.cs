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
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Width = Config.Default.MainWindow.Width,
                    Height = Config.Default.MainWindow.Height,
                    MinWidth = Config.Default.MainWindow.MinWidth,
                    MinHeight = Config.Default.MainWindow.MinHeight,
                    DataContext = viewModel ?? new MainWindowViewModel()
                };

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
        public static class CameraConfig
        {

            public static CameraConfigWindow Create()
            {
                var window = new CameraConfigWindow()
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    DataContext = new CameraConfigViewModel(),
                    Width = Config.Default.CameraConfigWindow.Width,
                    Height = Config.Default.CameraConfigWindow.Height,
                    ResizeMode = ResizeMode.NoResize,
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
