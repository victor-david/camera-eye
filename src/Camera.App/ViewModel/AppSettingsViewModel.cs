﻿using Restless.App.Camera.Core;
using Restless.Toolkit.Mvvm;

namespace Restless.App.Camera
{
    public class AppSettingsViewModel : WindowViewModel
    {
        #region Private
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets a boolean value that determines if proxy detection is disabled.
        /// </summary>
        public bool IsProxyDisabled
        {
            get => Config.IsProxyDetectionDisabled;
            set => Config.IsProxyDetectionDisabled = value;
        }
        #endregion

        /************************************************************************/

        #region Constructor
        public AppSettingsViewModel()
        {
            DisplayName = "Application Settings";
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Called when the view model is activated.
        /// </summary>
        protected override void OnActivated()
        {

        }

        /// <summary>
        /// Called when the view model is closing, that is when <see cref="ViewModelBase.SignalClosing"/> is called.
        /// </summary>
        protected override void OnClosing()
        {
            base.OnClosing();
            Config.SaveWindow(Config.WindowKey.AppSettings, WindowOwner);
        }
        #endregion

        /************************************************************************/

        #region Private methods
        #endregion
    }
}