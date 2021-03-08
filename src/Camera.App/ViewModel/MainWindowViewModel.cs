using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Tools.Mvvm;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Restless.App.Camera
{
    public class MainWindowViewModel : WindowViewModel
    {
        #region Private
        private const int MaxCamera = 20;
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
            Commands.Add("ManageCamera", RelayCommand.Create(RunManageCameraCommand, (p) => SelectedCamera != null));
            Commands.Add("RemoveCamera", RelayCommand.Create(RunRemoveCameraFromWallCommand, (p) => SelectedCamera != null));
            Commands.Add("DeleteCamera", RelayCommand.Create(RunDeleteCameraCommand, (p) => SelectedCamera != null));
            Commands.Add("CameraSelection", RelayCommand.Create(RunCameraSelectionChangedCommand));

            cameraList = new List<CameraRow>(DatabaseController.Instance.GetTable<CameraTable>().EnumerateAll());

            CameraList = new ListCollectionView(cameraList)
            {
                IsLiveSorting = true,
                CustomSort = new CameraRow.Comparer(),
            };

            CameraList.LiveSortingProperties.Add(nameof(CameraRow.Name));

            IsGridLayoutChecked = new bool[] { false, false, false, false, false, false, false };

            /* grid layout => index into IsGridSizeChecked */
            gridLayoutMap = new Dictionary<WallGridLayout, int>()
            {
                { WallGridLayout.OneByOne, 0 },
                { WallGridLayout.OneByTwo, 1 },
                { WallGridLayout.TwoByOne, 2 },
                { WallGridLayout.ThreeByOne, 3 },
                { WallGridLayout.TwoByTwo, 4 },
                { WallGridLayout.ThreeByTwo, 5 },
                { WallGridLayout.ThreeByThree, 6 }
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

            /* Close other windows. They aren't connected via owner to this window */
            foreach (var window in Application.Current.Windows.OfType<Window>())
            {
                if (window != WindowOwner) window.Close();
            }

            Config.SaveWindow(Config.WindowKey.Main, WindowOwner);
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

        private void RunCameraSelectionChangedCommand(object parm)
        {
            if (parm is CameraRow camera)
            {
                PushCommand = PushCommand.Create(PushCommandType.ShowCameraLocation, camera.Id);
            }

        }
        private void RunAddCameraCommand(object parm)
        {
            CameraRow newRow = DatabaseController.Instance.GetTable<CameraTable>().Add();
            cameraList.Add(newRow);
            CameraList.Refresh();
        }

        private void RunManageCameraCommand(object parm)
        {
            if (SelectedCamera != null)
            {
                bool isTop = IsTopmost;
                IsTopmost = false;
                SelectedCamera.PropertyChanged += SelectedCameraPropertyChanged;
                WindowFactory.CameraManage.Create(SelectedCamera).ShowDialog();
                SelectedCamera.PropertyChanged -= SelectedCameraPropertyChanged;
                IsTopmost = isTop;
            }
        }

        private void SelectedCameraPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == CameraTable.Defs.Columns.Flags && sender is CameraRow camera)
            {
                PushCommand = PushCommand.Create(PushCommandType.UpdateStatusBanner, camera.Id);
                PushCommand = PushCommand.Create(PushCommandType.UpdateOrientation, camera.Id);
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
            bool isTop = IsTopmost;
            IsTopmost = false;
            WindowFactory.AppSettings.Create().ShowDialog();
            IsTopmost = isTop;
        }
        #endregion
    }
}