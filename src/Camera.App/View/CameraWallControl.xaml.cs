using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Restless.App.Camera
{
    /// <summary>
    /// Interaction logic for CameraWallControl.xaml
    /// </summary>
    public partial class CameraWallControl : Grid
    {
        #region Constructor
        public CameraWallControl()
        {
            InitializeComponent();
            Background = Brushes.Transparent;
            CameraListHeader.IsVisibleChanged += CameraListHeaderIsVisibleChanged;
            InitializeCameraList();
            Loaded += CameraWallControlLoaded;
        }
        #endregion

        /************************************************************************/

        #region Rows / columns
        /// <summary>
        /// Gets or sets the number of rows.
        /// </summary>
        public int Rows
        {
            get => (int)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Rows"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register
            (
                nameof(Rows), typeof(int), typeof(CameraWallControl), new PropertyMetadata(0, OnGridSpecificationChanged, OnCoerceRows)
            );

        private static object OnCoerceRows(DependencyObject d, object baseValue)
        {
            int value = (int)baseValue;
            return Math.Min(Math.Max(value, 1), 3);
        }

        /// <summary>
        /// Gets or sets the number of columns.
        /// </summary>
        public int Columns
        {
            get => (int)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Columns"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register
            (
                nameof(Columns), typeof(int), typeof(CameraWallControl), new PropertyMetadata(0, OnGridSpecificationChanged, OnCoerceColumns)
            );

        private static object OnCoerceColumns(DependencyObject d, object baseValue)
        {
            int value = (int)baseValue;
            return Math.Min(Math.Max(value, 1), 3);
        }

        private static void OnGridSpecificationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
            if (CameraList.DragCursor != null)
            {
                e.UseDefaultCursors = false;
                Win32Point w32Mouse = new Win32Point();
                GetCursorPos(ref w32Mouse);
                CameraList.DragCursor.Left = w32Mouse.X + 2;
                CameraList.DragCursor.Top = w32Mouse.Y - (CameraList.DragCursor.ActualHeight / 2);
                CameraList.DragCursor.Opacity = (e.Effects == DragDropEffects.Move) ? 1.0 : 0.35;
                e.Handled = true;
            }
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

            for (int k = 0; k < Rows; k++)
            {
                RowDefinitions.Add(new RowDefinition());
            }

            for (int k = 0; k < Columns; k++)
            {
                ColumnDefinitions.Add(new ColumnDefinition());
            }

            ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(0, GridUnitType.Auto)
            });

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    if (GetCameraHost(row, col) == null)
                    {
                        Children.Add(new CameraHostBorder(row, col));
                    }
                }
            }

            SetRow(CameraListHeader, 0);
            SetColumn(CameraListHeader, Columns);
            SetRowSpan(CameraListHeader, Rows);
        }

        private void InitializeCameraList()
        {
            // TODO - make list auto update
            CameraList.ItemsSource = DatabaseController.Instance.GetTable<CameraTable>().EnumerateAll();
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
            double rowHeight = ActualHeight / Rows;
            double colWidth = (ActualWidth - ColumnDefinitions[Columns].ActualWidth) / Columns;
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
