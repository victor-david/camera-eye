using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
            //CameraListHeader.IsVisibleChanged += CameraListHeaderIsVisibleChanged;
            rows = columns = 1;
            //InitializeCameraList();
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

        #region Protected methods
        /// <summary>
        /// Called when the give feedback event is raised.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
            //if (CameraList.DragCursor != null)
            //{
            //    e.UseDefaultCursors = false;
            //    Win32Point w32Mouse = new Win32Point();
            //    GetCursorPos(ref w32Mouse);
            //    CameraList.DragCursor.Left = w32Mouse.X + 2;
            //    CameraList.DragCursor.Top = w32Mouse.Y - (CameraList.DragCursor.ActualHeight / 2);
            //    CameraList.DragCursor.Opacity = (e.Effects == DragDropEffects.Move) ? 1.0 : 0.35;
            //    e.Handled = true;
            //}
        }

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

            //ColumnDefinitions.Add(new ColumnDefinition()
            //{
            //    Width = new GridLength(0, GridUnitType.Auto)
            //});

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    if (GetCameraHost(row, col) == null)
                    {
                        Children.Add(new CameraHostBorder(row, col));
                    }
                }
            }

            Debug.WriteLine($"Grid has {Children.Count} children");
            CleanExtraSlots();
            Debug.WriteLine($"Grid has {Children.Count} children");

            //Debug.WriteLine($"Set List to row 0, col {columns} with row span {rows}");
            //SetRow(CameraListHeader, 0);
            //SetColumn(CameraListHeader, columns);
            //SetRowSpan(CameraListHeader, rows);
        }

        private void CleanExtraSlots()
        {
            //if (Children.Count > TotalSlots)
            {
                //Children.Remove(CameraListHeader);

                while (Children.Count > TotalSlots)
                {
                    int idx = Children.Count - 1;
                    if (Children[idx] is CameraHostBorder host)
                    {
                        if (host.CameraControl != null)
                        {
                            host.CameraControl.IsVideoRunning = false;
                        }
                    }
                    Children.RemoveAt(idx);
                }
                //int numToRemove = Children.Count - TotalSlots;
                //Debug.WriteLine($"More children than slots {TotalSlots} Remove {numToRemove}");
                //List<int> indices = new List<int>();
                //for (int idx = Children.Count - 1; idx >= 0; idx--)
                //{
                //    if (Children[idx] is CameraHostBorder host && indices.Count < numToRemove)
                //    {
                //        if (host.CameraControl != null)
                //        {
                //            host.CameraControl.IsVideoRunning = false;
                //        }
                //        indices.Add(idx);
                //    }
                //}
                //foreach (int idx in indices)
                //{
                //    Children.RemoveAt(idx);
                //}
                //Children.Add(CameraListHeader);
            }
        }

        //private void InitializeCameraList()
        //{
        //    // TODO - make list auto update
        //    CameraList.ItemsSource = DatabaseController.Instance.GetTable<CameraTable>().EnumerateAll();
        //}

        private void CameraWallControlLoaded(object sender, RoutedEventArgs e)
        {
            ShowGridLines = true;
            foreach (CameraRow camera in DatabaseController.Instance.GetTable<CameraTable>().EnumerateFlaggedForWall())
            {
                //if (GetCameraHost(camera.WallRow, camera.WallColumn) is CameraHostBorder host)
                //{
                //    AddCameraToWall(host, camera);
                //}
            }
            Loaded -= CameraWallControlLoaded;
        }

        private void CameraListHeaderIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            foreach (var child in Children.OfType<CameraHostBorder>())
            {
                child.SetActivationState((bool)e.NewValue);
            }
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
            foreach (var child in Children.OfType<CameraHostBorder>())
            {
                if (child.CameraControl is CameraControl cameraControl)
                {
                    if (cameraControl.Camera.Id == camera.Id)
                    {
                        return child;
                    }
                }
            }
            return null;
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        };


        #endregion
    }
}
