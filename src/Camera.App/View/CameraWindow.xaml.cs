using Restless.App.Camera.Core;
using Restless.Tools.Mvvm;
using System.ComponentModel;
using System.Windows;

namespace Restless.App.Camera
{
    /// <summary>
    /// Interaction logic for CameraWindow.xaml
    /// </summary>
    public partial class CameraWindow : AppWindow
    {
        public CameraWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!e.Cancel)
            {
                (DataContext as ViewModelBase)?.SignalClosing();
            }
        }

        /// <summary>
        /// Called when the window is loaded
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args</param>
        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            (DataContext as ViewModelBase)?.Activate();
        }
    }
}