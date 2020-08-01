using Restless.App.Camera.Core;
using Restless.Tools.Mvvm;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Restless.App.Camera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AppWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!e.Cancel)
            {
                /* Close other windows. They aren't connected via owner to this window */
                foreach (var window in Application.Current.Windows.OfType<Window>())
                {
                    if (window != this) window.Close();
                }
                (DataContext as ViewModelBase)?.SignalClosing();
            }
        }
    }
}