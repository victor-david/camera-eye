using Restless.Tools.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;

namespace Restless.App.Camera
{
    /// <summary>
    /// View model class for windows.
    /// </summary>
    public abstract class WindowViewModel : ApplicationViewModel
    {
        #region Properties
        /// <summary>
        /// Gets the window associated with this view model
        /// </summary>
        public Window WindowOwner
        { 
            get;
            private set;
        }

        /// <summary>
        /// Gets the command to close the window.
        /// </summary>
        public ICommand CloseWindowCommand { get; }

        /// <summary>
        /// Gets the command to toggle window state.
        /// </summary>
        public ICommand ToggleWindowStateCommand { get; }

        /// <summary>
        /// Gets the command to minimize the window
        /// </summary>
        public ICommand SetWindowStateMinimizedCommand { get; }
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the<see cref="WindowViewModel"/> class.
        /// </summary>
        protected WindowViewModel()
        {
            CloseWindowCommand = RelayCommand.Create((p) => WindowOwner.Close());
            ToggleWindowStateCommand = RelayCommand.Create(RunChangeWindowStateCommand);
            SetWindowStateMinimizedCommand = RelayCommand.Create((p) => WindowOwner.WindowState = WindowState.Minimized);
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Sets <see cref="WindowOwner"/> to the specified owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public void SetWindowOwner(Window owner)
        {
            WindowOwner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private void RunChangeWindowStateCommand(object parm)
        {
            WindowOwner.WindowState = WindowOwner.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        #endregion
    }
}