using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Camera.Contracts;
using Restless.Tools.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Restless.App.Camera
{
    public class CameraManageViewModel : WindowViewModel
    {
        #region Private
        private ICameraPlugin plugin;
        private string errorText;
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets the camera row object.
        /// </summary>
        public CameraRow Camera
        {
            get;
        }

        private ICameraPlugin Plugin
        {
            get => plugin;
            set
            {
                plugin = value;
                OnPropertyChanged(nameof(PluginIsSettings));
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraSettings.
        /// </summary>
        public bool PluginIsSettings
        {
            get => plugin is ICameraSettings;
        }

        /// <summary>
        /// Gets error text
        /// </summary>
        public string ErrorText
        {
            get => errorText;
            private set
            {
                SetProperty(ref errorText, value);
                OnPropertyChanged(nameof(HaveError));
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates if an error message is present.
        /// </summary>
        public bool HaveError
        {
            get => !string.IsNullOrEmpty(ErrorText);
        }

        /// <summary>
        /// Gets an enumerable of discovered plugins.
        /// </summary>
        public IEnumerable<PluginRow> CameraPlugins => DatabaseController.Instance.GetTable<PluginTable>().EnumeratePlugins();

        #endregion

        /************************************************************************/

        #region Constructor
        public CameraManageViewModel(CameraRow camera)
        {
            Camera = camera ?? throw new ArgumentNullException(nameof(camera));
            DisplayName = Camera.Name;
            Commands.Add("ChangeStatusBanner", RelayCommand.Create(RunChangeStatusBannerCommand));
            Camera.PropertyChanged += CameraPropertyChanged;
            CreatePlugin();
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Called when the view model is closing, that is when <see cref="ViewModelBase.SignalClosing"/> is called.
        /// </summary>
        protected override void OnClosing()
        {
            base.OnClosing();
            Camera.PropertyChanged -= CameraPropertyChanged;
        }
        #endregion

        /************************************************************************/

        #region Private methods

        private void CameraPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == CameraTable.Defs.Columns.Name)
            {
                DisplayName = Camera.Name;
            }

            if (e.PropertyName == CameraTable.Defs.Columns.PluginId)
            {
                DestroyPlugin();
                CreatePlugin();
            }
        }

        private void CreatePlugin()
        {
            try
            {
                ErrorText = null;
                if (Camera.PluginId != PluginTable.Defs.Values.NullPluginId)
                {
                    Plugin = PluginFactory.Create(Camera);
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private void DestroyPlugin()
        {
            if (Plugin != null)
            {
                Plugin.StopVideoAsync();
                Plugin = null;
            }
        }

        private void RunChangeStatusBannerCommand(object parm)
        {
            if (parm is CameraFlags flag)
            {
                Camera.ChangeFlags(flag, StatusFlagsToRemove(flag));
            }
        }

        private CameraFlags StatusFlagsToRemove(CameraFlags flag)
        {
            return flag switch
            {
                CameraFlags.StatusTop => CameraFlags.StatusBottom,
                CameraFlags.StatusBottom => CameraFlags.StatusTop,
                _ => CameraFlags.StatusTop | CameraFlags.StatusBottom,
            };
        }
        #endregion
    }
}