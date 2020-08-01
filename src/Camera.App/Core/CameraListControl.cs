using Restless.App.Database.Tables;
using System;
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
        public Window DragCursor
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the dra cursor brush.
        /// </summary>
        public Brush DragCursorBrush
        {
            get => (Brush)GetValue(DragCursorBrushProperty);
            set => SetValue(DragCursorBrushProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="DragCursorBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DragCursorBrushProperty = DependencyProperty.Register
            (
                nameof(DragCursorBrush), typeof(Brush), typeof(CameraListControl), new PropertyMetadata(Brushes.DarkBlue)
            );
        #endregion

        /************************************************************************/

        #region Protected methods (drag / drop)
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
        #endregion

        /************************************************************************/

        #region Private methods
        private Window CreateDragCursor(CameraRow camera)
        {
            return new Window()
            {
                Background = DragCursorBrush,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Topmost = true,
                ShowInTaskbar = false,
                Width = 186,
                Height = 32,
                Content = new Border()
                {
                    BorderBrush = Brushes.LightGray,
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
        #endregion
    }
}