using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Tools.Mvvm;
using System.Collections.Generic;
using System.Windows.Data;

namespace Restless.App.Camera
{
    public class MainWindowViewModel : WindowViewModel
    {
        #region Private
        private const int MaxCamera = 9;
        private readonly List<CameraRow> cameraList;
        private readonly Dictionary<WallGridLayout, int> gridLayoutMap;
        private PushCommand pushCommand;
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
        /// Gets the list collection view that displays the list of cameras.
        /// </summary>
        public ListCollectionView CameraList
        {
            get;
        }

        /// <summary>
        /// Gets or sets the currently selected camera.
        /// </summary>
        public CameraRow SelectedCamera
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the push command associated with the camera wall control.
        /// </summary>
        public PushCommand PushCommand
        {
            get => pushCommand;
            private set => SetProperty(ref pushCommand, value);
        }

        /// <summary>
        /// Gets an array of boolean values that determine which grid layout menu item is checked.
        /// </summary>
        public bool[] IsGridLayoutChecked
        {
            get;
        }

        /// <summary>
        /// Gets a boolean value that determines if the window is topmost.
        /// </summary>
        public bool IsTopmost
        {
            get => Config.MainWindowTopmost;
            private set
            {
                Config.MainWindowTopmost = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /************************************************************************/

        #region Constructor
        public MainWindowViewModel()
        {
            DisplayName = "Camera Eye";
            Commands.Add("ChangeGridLayout", RelayCommand.Create((p) => GridLayout = (WallGridLayout)p));
            Commands.Add("ToggleCameraList", RelayCommand.Create((p) => IsCameraListVisible = !IsCameraListVisible));
            Commands.Add("CloseCameraList", RelayCommand.Create((p) => IsCameraListVisible = false));
            Commands.Add("OpenAppSettings", RelayCommand.Create(RunOpenAppSettingsCommand));
            Commands.Add("ToggleTopmost", RelayCommand.Create((p) => IsTopmost = !IsTopmost));

            Commands.Add("AddCamera", RelayCommand.Create(RunAddCameraCommand, (p) => CameraList.Count < MaxCamera));
            Commands.Add("EditCamera", RelayCommand.Create(RunEditCameraCommand, (p) => SelectedCamera != null));
            Commands.Add("RemoveCamera", RelayCommand.Create(RunRemoveCameraFromWallCommand, (p) => SelectedCamera != null));
            Commands.Add("DeleteCamera", RelayCommand.Create(RunDeleteCameraCommand, (p) => SelectedCamera != null));

            cameraList = new List<CameraRow>(DatabaseController.Instance.GetTable<CameraTable>().EnumerateAll());

            CameraList = new ListCollectionView(cameraList)
            {
                CustomSort = new CameraRow.Comparer()
            };

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
            Config.SaveMainWindow(WindowOwner);
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

        private void RunAddCameraCommand(object parm)
        {
            CameraRow newRow = DatabaseController.Instance.GetTable<CameraTable>().Add();
            cameraList.Add(newRow);
            CameraList.Refresh();
        }

        private void RunEditCameraCommand(object parm)
        {
            if (SelectedCamera != null)
            {
                WindowFactory.CameraEdit.Create(SelectedCamera).ShowDialog();
                CameraList.Refresh();
            }
        }

        private void RunRemoveCameraFromWallCommand(object parm)
        {
            if (SelectedCamera != null)
            {
                PushCommand = PushCommand.Create(PushCommandType.RemoveFromWall, SelectedCamera.Id);
            }
        }

        private void RunDeleteCameraCommand(object parm)
        {
            if (SelectedCamera != null && Messages.ShowYesNo($"Remove camera {SelectedCamera.Name}?"))
            {
                PushCommand = PushCommand.Create(PushCommandType.RemoveFromWall, SelectedCamera.Id);
                cameraList.Remove(SelectedCamera);
                SelectedCamera.Row.Delete();
                DatabaseController.Instance.Save();
                SelectedCamera = null;
                CameraList.Refresh();
            }
        }

        private void RunOpenAppSettingsCommand(object parm)
        {
        }
        #endregion
    }
}