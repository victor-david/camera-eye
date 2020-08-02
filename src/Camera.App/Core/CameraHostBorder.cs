using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Represents the border that hosts a <see cref="CameraControl"/>
    /// </summary>
    public class CameraHostBorder : Border
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CameraHostBorder"/> class.
        /// </summary>
        public CameraHostBorder()
        {
            AllowDrop = true;
            Background = Brushes.Transparent;
            BorderBrush = Brushes.DarkGray;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraHostBorder"/> class.
        /// </summary>
        /// <param name="gridRow">The grid row to place the border.</param>
        /// <param name="gridCol">The grid column to place the border.</param>
        public CameraHostBorder(int gridRow, int gridCol) : this()
        {
            Grid.SetRow(this, gridRow);
            Grid.SetColumn(this, gridCol);
        }
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets or sets the camera control associated with this host border.
        /// </summary>
        public CameraControl CameraControl
        {
            get => Child as CameraControl;
            set => Child = value;
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Saves the camera location (if hosted) to the specified row and column.
        /// </summary>
        /// <param name="row">The row</param>
        /// <param name="col">The column</param>
        public void MoveCameraLocation(int row, int col)
        {
            if (CameraControl != null)
            {
                CameraControl.Camera.WallRow = row;
                CameraControl.Camera.WallColumn = col;
            }
        }

        /// <summary>
        /// Prepares this host border for removal by stopping the camera video
        /// if a camera control has been assigned.
        /// </summary>
        public void PrepareForRemoval()
        {
            if (CameraControl != null)
            {
                CameraControl.IsVideoRunning = false;
            }
        }

        /// <summary>
        /// Sets the activation state of the border.
        /// </summary>
        /// <param name="state">true to set activated state, false to set unactivated state.</param>
        public void SetActivationState(bool state)
        {
            double thickness = state ? 1.0 : 0.0;
            BorderThickness = new Thickness(thickness);
            Margin = new Thickness(thickness);
        }
        #endregion
    }
}
