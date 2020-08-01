using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Tools.Mvvm;
using System.Collections.Generic;
using System.Windows;

namespace Restless.App.Camera
{
    public class MainWindowViewModel : WindowViewModel
    {
        #region Private
        private WallGridLayout gridLayout;
        private bool isCameraListVisible;
        private Dictionary<WallGridLayout, int> gridLayoutMap;
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets the wall grid layout.
        /// </summary>
        public WallGridLayout GridLayout
        {
            get => gridLayout;
            private set => SetProperty(ref gridLayout, value);
        }

        /// <summary>
        /// Gets the list of cameras
        /// </summary>
        public IEnumerable<CameraRow> CameraList
        {
            get;
        }

        /// <summary>
        /// Gets a value that determines is the camera list is visible.
        /// </summary>
        public bool IsCameraListVisible
        {
            get => isCameraListVisible;
            private set => SetProperty(ref isCameraListVisible, value);
        }

        public bool[] IsGridSizeChecked
        {
            get;
        }
        #endregion

        /************************************************************************/

        #region Constructor
        public MainWindowViewModel()
        {
            DisplayName = "Camera Eye";
            Commands.Add("ChangeGridLayout", RelayCommand.Create(RunChangeGridSizeCommand));
            Commands.Add("OpenCameraList", RelayCommand.Create((p) => IsCameraListVisible = !IsCameraListVisible));
            Commands.Add("OpenCameraConfig", RelayCommand.Create(RunOpenCameraConfigWindowCommand));
            Commands.Add("OpenAppSettings", RelayCommand.Create(RunOpenAppSettingsCommand));

            CameraList = DatabaseController.Instance.GetTable<CameraTable>().EnumerateAll();

            IsGridSizeChecked = new bool[] { false, false, false, false, false, false };

            /* grid layout => index into IsGridSizeChecked */
            gridLayoutMap = new Dictionary<WallGridLayout, int>()
            {
                { WallGridLayout.OneByOne, 0 },
                { WallGridLayout.OneByTwo, 1 },
                { WallGridLayout.TwoByOne, 2 },
                { WallGridLayout.TwoByTwo, 3 },
                { WallGridLayout.ThreeByTwo, 4 },
                { WallGridLayout.ThreeByThree, 5 }
            };

            RunChangeGridSizeCommand(WallGridLayout.TwoByTwo);
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Called when the view model is closing, that is when <see cref="ViewModelBase.SignalClosing"/> is called.
        /// </summary>
        protected override void OnClosing()
        {
            base.OnClosing();
            if (WindowOwner != null)
            {
                if (WindowOwner.WindowState != WindowState.Maximized)
                {
                    Config.MainWindowWidth = (int)WindowOwner.Width;
                    Config.MainWindowHeight = (int)WindowOwner.Height;
                }
                if (WindowOwner.WindowState != WindowState.Minimized)
                {
                    Config.MainWindowState = WindowOwner.WindowState;
                }
            }
        }
        #endregion

        /************************************************************************/

        #region Private methods

        private void RunChangeGridSizeCommand(object parm)
        {
            if (parm is WallGridLayout layout && gridLayoutMap.ContainsKey(layout))
            {
                GridLayout = layout;
                for (int idx = 0; idx < IsGridSizeChecked.Length; idx++)
                {
                    IsGridSizeChecked[idx] = idx == gridLayoutMap[layout];
                }
                OnPropertyChanged(nameof(IsGridSizeChecked));
            }
        }

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