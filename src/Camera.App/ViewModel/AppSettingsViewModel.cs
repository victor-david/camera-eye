using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Camera.Contracts;
using Restless.Tools.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Restless.App.Camera
{
    public class AppSettingsViewModel : WindowViewModel
    {
        #region Private
        #endregion

        /************************************************************************/

        #region Properties
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