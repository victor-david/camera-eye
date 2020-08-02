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
        private Dictionary<WallGridLayout, int> gridLayoutMap;
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets the wall grid layout.
        /// </summary>
        public WallGridLayout GridLayout
        {
            get => Config.GridLayout;
            private set
            {
                Config.GridLayout = value;
                UpdateGridLayoutChecked(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value that determines is the camera list is visible.
        /// </summary>
        public bool IsCameraListVisible
        {
            get => Config.IsCameraListVisible;
            private set
            {
                Config.IsCameraListVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the list of cameras
        /// </summary>
        public IEnumerable<CameraRow> CameraList
        {
            get;
        }

        /// <summary>
        /// Gets an array of boolean values that determine which grid layout menu item is checked.
        /// </summary>
        public bool[] IsGridLayoutChecked
        {
            get;
        }
        #endregion

        /************************************************************************/

        #region Constructor
        public MainWindowViewModel()
        {
            DisplayName = "Camera Eye";
            Commands.Add("ChangeGridLayout", RelayCommand.Create((p) => GridLayout = (WallGridLayout)p));
            Commands.Add("OpenCameraList", RelayCommand.Create((p) => IsCameraListVisible = !IsCameraListVisible));
            Commands.Add("OpenCameraConfig", RelayCommand.Create(RunOpenCameraConfigWindowCommand));
            Commands.Add("OpenAppSettings", RelayCommand.Create(RunOpenAppSettingsCommand));

            CameraList = DatabaseController.Instance.GetTable<CameraTable>().EnumerateAll();

            IsGridLayoutChecked = new bool[] { false, false, false, false, false, false };

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

            UpdateGridLayoutChecked(Config.GridLayout);
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

        private void UpdateGridLayoutChecked(WallGridLayout layout)
        {
            if (gridLayoutMap.ContainsKey(layout))
            {
                for (int idx = 0; idx < IsGridLayoutChecked.Length; idx++)
                {
                    IsGridLayoutChecked[idx] = idx == gridLayoutMap[layout];
                }
                OnPropertyChanged(nameof(IsGridLayoutChecked));
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