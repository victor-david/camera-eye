using Restless.App.Camera.Core;
using Restless.Tools.Mvvm;

namespace Restless.App.Camera
{
    public class MainWindowViewModel : WindowViewModel
    {
        #region Private
        private int rows;
        private int columns;
        private bool isCameraListVisible;
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets the number of rows for the camera wall.
        /// </summary>
        public int Rows
        {
            get => rows;
            private set => SetProperty(ref rows, value);
        }

        /// <summary>
        /// Gets the number of columns for the camera wall.
        /// </summary>
        public int Columns
        {
            get => columns;
            private set => SetProperty(ref columns, value);
        }

        /// <summary>
        /// Gets a value that determines is the camera list is viaible.
        /// </summary>
        public bool IsCameraListVisible
        {
            get => isCameraListVisible;
            private set => SetProperty(ref isCameraListVisible, value);
        }
        #endregion

        /************************************************************************/

        #region Constrcutor
        public MainWindowViewModel()
        {
            DisplayName = "Camera Eye";
            Commands.Add("OpenCameraList", RelayCommand.Create((p) => IsCameraListVisible = !IsCameraListVisible));
            Commands.Add("OpenCameraConfig", RelayCommand.Create(RunOpenCameraConfigWindowCommand));
            Commands.Add("OpenAppSettings", RelayCommand.Create(RunOpenAppSettingsCommand));
            Rows = 2;
            Columns = 2;
        }
        #endregion

        /************************************************************************/

        #region Private methods

        private void RunOpenCameraConfigWindowCommand(object parm)
        {
            WindowFactory.CameraConfig.Create().ShowDialog();
        }

        private void RunOpenAppSettingsCommand(object parm)
        {
        }
        #endregion
    }
}