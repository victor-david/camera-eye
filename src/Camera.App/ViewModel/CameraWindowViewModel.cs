using Restless.App.Database.Tables;
using Restless.Tools.Mvvm;
using System;

namespace Restless.App.Camera
{
    public class CameraWindowViewModel : WindowViewModel
    {
        #region Private
        private bool isTopmost;
        private bool isVideoRunning;
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets the camera
        /// </summary>
        public CameraRow Camera
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value that determines if the video is running.
        /// </summary>
        public bool IsVideoRunning
        {
            get => isVideoRunning;
            set => SetProperty(ref isVideoRunning, value);
        }

        /// <summary>
        /// Gets a value that determines if the camera window is topmost.
        /// </summary>
        public bool IsTopmost
        {
            get => isTopmost;
            private set => SetProperty(ref isTopmost, value);
        }
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the<see cref="CameraWindowViewModel"/> class.
        /// </summary>
        /// <param name="camera">The camera</param>
        public CameraWindowViewModel(CameraRow camera)
        {
            Camera = camera ?? throw new ArgumentNullException(nameof(camera));
            DisplayName = Camera.Name;

            /* Initialize video commands */
            Commands.Add("StartVideo", RelayCommand.Create((p) => IsVideoRunning = true));
            Commands.Add("StopVideo", RelayCommand.Create((p) => IsVideoRunning = false));

            /* Initialize other standard commands */
            Commands.Add("SetWindowOnTop", RelayCommand.Create((p) => IsTopmost = !IsTopmost));
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Called when the view model is activated.
        /// </summary>
        protected override void OnActivated()
        {
            IsVideoRunning = true;
        }

        /// <summary>
        /// Called when the view model has been signaled to close.
        /// </summary>
        protected override void OnClosing()
        {
            base.OnClosing();
            IsVideoRunning = false;
        }
        #endregion
    }
}