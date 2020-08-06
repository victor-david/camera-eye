using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Interaction logic for CameraWallControl.xaml
    /// </summary>
    public partial class CameraWallControl : Grid
    {
        #region Private
        private int rows;
        private int columns;
        #endregion

        /************************************************************************/

        #region Constructor
        public CameraWallControl()
        {
            Background = Brushes.Transparent;
            rows = columns = 1;
            Loaded += CameraWallControlLoaded;
        }
        #endregion

        /************************************************************************/

        #region Grid layout
        /// <summary>
        /// Gets the total number of camera slots.
        /// </summary>
        private int TotalSlots => rows * columns;

        /// <summary>
        /// Gets or sets the grid layout
        /// </summary>
        public WallGridLayout GridLayout
        {
            get => (WallGridLayout)GetValue(GridLayoutProperty);
            set => SetValue(GridLayoutProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="GridLayout"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GridLayoutProperty = DependencyProperty.Register
            (
                nameof(GridLayout), typeof(WallGridLayout), typeof(CameraWallControl), new PropertyMetadata(WallGridLayout.None, OnGridLayoutChanged)
            );

        private static void OnGridLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CameraWallControl control)
            {
                control.InitializeGrid();
            }
        }
        #endregion

        /************************************************************************/

        #region IsActivated for drop
        /// <summary>
        /// Gets or sets a boolean value that indicates if the control is
        /// activated for drop. This change visual aspects of the control, not functionality.
        /// </summary>
        public bool IsActivatedForDrop
        {
            get => (bool)GetValue(IsActivatedForDropProperty);
            set => SetValue(IsActivatedForDropProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="IsActivatedForDrop"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsActivatedForDropProperty = DependencyProperty.Register
            (
                nameof(IsActivatedForDrop), typeof(bool), typeof(CameraWallControl), new PropertyMetadata(false, OnIsActivatedForDropChanged)
            );

        private static void OnIsActivatedForDropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CameraWallControl control)
            {
                control.ChangeVisualForDropActivationState((bool)e.NewValue);
            }
        }
        #endregion

        /************************************************************************/

        #region PushCommand
        /// <summary>
        /// Gets or sets the push command.
        /// </summary>
        public PushCommand PushCommand
        {
            get => (PushCommand)GetValue(PushCommandProperty);
            set => SetValue(PushCommandProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="PushCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PushCommandProperty = DependencyProperty.Register
            (
                nameof(PushCommand), typeof(PushCommand), typeof(CameraWallControl), new PropertyMetadata(null, OnPushCommandChanged)
            );

        private static void OnPushCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CameraWallControl control && e.NewValue is PushCommand command)
            {
                control.ExecutePushCommand(command);
            }
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Called when the drop operation has completed
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            if (e.Data.GetData(typeof(CameraRow)) as CameraRow is CameraRow camera)
            {
                AddCameraToWall(camera, e.GetPosition(this));
            }
        }
        #endregion

        /************************************************************************/

        #region Private methods (initialization)
        private void InitializeGrid()
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            rows = GridLayout.ToRow();
            columns = GridLayout.ToColumn();

            for (int k = 0; k < rows; k++)
            {
                RowDefinitions.Add(new RowDefinition());
            }

            for (int k = 0; k < columns; k++)
            {
                ColumnDefinitions.Add(new ColumnDefinition());
            }

            while (Children.Count < TotalSlots)
            {
                Children.Add(new CameraHostBorder());
            }

            while (Children.Count > TotalSlots)
            {
                int idx = Children.Count - 1;
                (Children[idx] as CameraHostBorder)?.PrepareForRemoval();
                Children.RemoveAt(idx);
            }

            int childIndex = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    SetRow(Children[childIndex], row);
                    SetColumn(Children[childIndex], col);
                    (Children[childIndex] as CameraHostBorder)?.MoveCameraLocation(row, col);
                    childIndex++;
                }
            }
            ChangeVisualForDropActivationState(IsActivatedForDrop);
        }

        private void CameraWallControlLoaded(object sender, RoutedEventArgs e)
        {
            foreach (CameraRow camera in DatabaseController.Instance.GetTable<CameraTable>().EnumerateFlaggedForWall())
            {
                if (GetCameraHost(camera.WallRow, camera.WallColumn) is CameraHostBorder host)
                {
                    AddCameraToWall(host, camera);
                }
            }
            Loaded -= CameraWallControlLoaded;
        }
        #endregion

        /************************************************************************/

        #region Private methods (drag / drop support)
        private Tuple<int, int> GetGridCellByPoint(Point point)
        {
            double rowHeight = ActualHeight / rows;
            double colWidth = ActualWidth / columns;
            int row = (int)(point.Y / rowHeight);
            int col = (int)(point.X / colWidth);
            return new Tuple<int, int>(row, col);
        }

        private void AddCameraToWall(CameraRow camera, Point dropPoint)
        {
            Tuple<int, int> dropCell = GetGridCellByPoint(dropPoint);
            if (GetCameraHost(dropCell.Item1, dropCell.Item2) is CameraHostBorder host)
            {
                /* see if there's already a control in the drop cell */
                if (host.CameraControl is CameraControl hostCameraControl)
                {
                    /* if same camera, nothing to do. bail. */
                    if (hostCameraControl.Camera.Id == camera.Id)
                    {
                        return;
                    }
                    /* otherwise, stop the video, remove camera from wall */
                    hostCameraControl.IsVideoRunning = false;
                    hostCameraControl.Camera.RemoveWallProperties();
                }

                /* if camera is hosted in another cell, take it out. */
                if (GetCameraHost(camera) is CameraHostBorder prevHost)
                {
                    prevHost.CameraControl.IsVideoRunning = false;
                    prevHost.CameraControl = null;
                }

                /* ready to place a camera control in the host */
                camera.SetWallProperties(dropCell.Item1, dropCell.Item2);
                AddCameraToWall(host, camera);
            }
        }

        private void AddCameraToWall(CameraHostBorder host, CameraRow camera)
        {
            CameraControl cameraControl = new CameraControl() { Camera = camera };
            host.CameraControl = cameraControl;
            host.CameraControl.IsVideoRunning = true;
        }

        /// <summary>
        /// Gets the <see cref="CameraHostBorder"/> in the specified grid cell.
        /// </summary>
        /// <param name="row">The grid row.</param>
        /// <param name="col">The grid column.</param>
        /// <returns>The host, or null if there isn't one at the specified cell.</returns>
        private CameraHostBorder GetCameraHost(long row, long col)
        {
            foreach (var child in Children.OfType<CameraHostBorder>())
            {
                if (GetRow(child) == row && GetColumn(child) == col)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the <see cref="CameraHostBorder"/> that contains the specified camera.
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <returns>The host, or null if a no host with the specified camera</returns>
        private CameraHostBorder GetCameraHost(CameraRow camera)
        {
            return GetCameraHost(camera.Id);
        }

        /// <summary>
        /// Gets the <see cref="CameraHostBorder"/> that contains the camera with the specified id.
        /// </summary>
        /// <param name="camera">The camera</param>
        /// <returns>The host, or null if a no host with the specified camera</returns>
        private CameraHostBorder GetCameraHost(long id)
        {
            foreach (var child in Children.OfType<CameraHostBorder>())
            {
                if (child.CameraControl is CameraControl cameraControl)
                {
                    if (cameraControl.Camera.Id == id)
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        private void ChangeVisualForDropActivationState(bool activationState)
        {
            foreach (var child in Children.OfType<CameraHostBorder>())
            {
                child.SetActivationState(activationState);
            }
        }
        #endregion

        /************************************************************************/

        #region Private methods (other)
        private void RemoveCameraFromWall(long id)
        {
            if (GetCameraHost(id) is CameraHostBorder host)
            {
                host.CameraControl.IsVideoRunning = false;
                host.CameraControl.Camera.RemoveWallProperties();
                host.CameraControl = null;
            }
        }

        private void UpdateStatusBanner(long id)
        {
            if (GetCameraHost(id) is CameraHostBorder host)
            {
                host.CameraControl.StatusAlignment = GetStatusAlignment(host.CameraControl.Camera);
            }
        }

        private VerticalAlignment GetStatusAlignment(CameraRow camera)
        {
            if (camera.Flags.HasFlag(CameraFlags.StatusTop)) return VerticalAlignment.Top;
            if (camera.Flags.HasFlag(CameraFlags.StatusBottom)) return VerticalAlignment.Bottom;
            /* center causes trigger to hide the status banner */
            return VerticalAlignment.Center;
        }

        private void ExecutePushCommand(PushCommand command)
        {
            switch (command.CommandType)
            {
                case PushCommandType.RemoveFromWall:
                    RemoveCameraFromWall(command.Id);
                    break;
                case PushCommandType.UpdateStatusBanner:
                    UpdateStatusBanner(command.Id);
                    break;
            }
        }
        #endregion
    }
}