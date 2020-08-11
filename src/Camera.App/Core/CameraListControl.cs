using Restless.App.Database.Tables;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Represents the control to display the list of cameras that may be dragged onto the camera wall.
    /// </summary>
    public class CameraListControl : ListBox
    {
        #region Private
        private Point startPoint;
        private bool isDragInProgress;
        #endregion

        /************************************************************************/

        #region Constructors
        public CameraListControl()
        {
            AllowDrop = false;
        }
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets the drag cursor.
        /// </summary>
        public Window DragCursor
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the drag cursor border brush.
        /// </summary>
        public Brush DragCursorBorderBrush
        {
            get => (Brush)GetValue(DragCursorBorderBrushProperty);
            set => SetValue(DragCursorBorderBrushProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="DragCursorBorderBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DragCursorBorderBrushProperty = DependencyProperty.Register
            (
                nameof(DragCursorBorderBrush), typeof(Brush), typeof(CameraListControl), new PropertyMetadata(Brushes.DarkBlue)
            );

        /// <summary>
        /// Gets or sets a command to execute when the selection is changed.
        /// The selected item is passed as the parameter to the command.
        /// </summary>
        public ICommand SelectionChangedCommand
        {
            get => (ICommand)GetValue(SelectionChangedCommandProperty);
            set => SetValue(SelectionChangedCommandProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="SelectionChangedCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionChangedCommandProperty = DependencyProperty.Register
            (
                nameof(SelectionChangedCommand), typeof(ICommand), typeof(CameraListControl), new PropertyMetadata(null)
            );
        #endregion

        /************************************************************************/

        #region Protected methods

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (e.AddedItems.Count > 0)
            {
                SelectionChangedCommand?.Execute(e.AddedItems[0]);
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            startPoint = e.GetPosition(this);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            isDragInProgress = false;
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            if (!isDragInProgress && e.LeftButton == MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(this);

                if (Math.Abs(pos.X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(pos.Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (SelectedItem is CameraRow camera)
                    {
                        DragCursor = CreateDragCursor(camera);
                        DragCursor.Show();
                        isDragInProgress = true;
                        DragDrop.DoDragDrop(this, camera, DragDropEffects.Move);
                        isDragInProgress = false;
                        DragCursor.Close();
                        DragCursor = null;
                    }
                }
            }
        }

        /// <summary>
        /// Called when the give feedback event is raised.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
            if (DragCursor != null)
            {
                e.UseDefaultCursors = false;
                Win32Point w32Mouse = new Win32Point();
                GetCursorPos(ref w32Mouse);
                DragCursor.Left = w32Mouse.X + 2;
                DragCursor.Top = w32Mouse.Y - (DragCursor.ActualHeight / 2);
                DragCursor.Opacity = (e.Effects == DragDropEffects.Move) ? 1.0 : 0.35;
                e.Handled = true;
            }
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private Window CreateDragCursor(CameraRow camera)
        {
            return new Window()
            {
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Topmost = true,
                ShowInTaskbar = false,
                Width = 186,
                Height = 32,
                Content = new Border()
                {
                    BorderBrush = DragCursorBorderBrush,
                    BorderThickness = new Thickness(1.0),
                    Background = Application.Current.FindResource(SystemColors.HighlightBrushKey) as Brush,
                    Child = new TextBlock() 
                    {
                        Text = camera.Name,
                        Foreground = Brushes.Black,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                },
            };
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