using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Camera.Contracts;
using Restless.Tools.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Restless.App.Camera
{
    public class CameraManageViewModel : WindowViewModel
    {
        #region Private
        private ICameraPlugin plugin;
        private string errorText;
        private int brightness;
        private int contrast;
        private int hue;
        private int saturation;
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
                OnPropertyChanged(nameof(PluginIsColor));
                OnPropertyChanged(nameof(PluginIsNone));
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
        /// Gets a boolean value that indicates if the plugin supports ICameraColor.
        /// </summary>
        public bool PluginIsColor
        {
            get => plugin is ICameraColor;
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin does not support additional configuration.
        /// </summary>
        public bool PluginIsNone
        {
            get => !PluginIsSettings && !PluginIsColor;
        }

        /// <summary>
        /// Gets or sets the brightness.
        /// </summary>
        public int Brightness
        {
            get => brightness;
            set 
            { 
                SetProperty(ref brightness, value);
                if (Plugin is ICameraColor color) color.Brightness = value;
            }
        }

        /// <summary>
        /// Gets or sets the contrast.
        /// </summary>
        public int Contrast
        {
            get => contrast;
            set 
            {
                SetProperty(ref contrast, value);
                if (Plugin is ICameraColor color) color.Contrast = value;
            }
        }


        /// <summary>
        /// Gets or sets the hue.
        /// </summary>
        public int Hue
        {
            get => hue;
            set 
            { 
                SetProperty(ref hue, value);
                if (Plugin is ICameraColor color) color.Hue = value;
            }
        }

        /// <summary>
        /// Gets or sets the saturation.
        /// </summary>
        public int Saturation
        {
            get => saturation;
            set 
            {
                SetProperty(ref saturation, value);
                if (Plugin is ICameraColor color) color.Saturation = value;
            }
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
            Commands.Add("FlipOn", RelayCommand.Create((p) => (plugin as ICameraSettings)?.SetFlip(true)));
            Commands.Add("FlipOff", RelayCommand.Create((p) => (plugin as ICameraSettings)?.SetFlip(false)));

            Commands.Add("MirrorOn", RelayCommand.Create((p) => (plugin as ICameraSettings)?.SetMirror(true)));
            Commands.Add("MirrorOff", RelayCommand.Create((p) => (plugin as ICameraSettings)?.SetMirror(false)));

            Brightness = Contrast = Hue = Saturation = 50;

            Camera.PropertyChanged += CameraPropertyChanged;
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Called when the view model is activated.
        /// </summary>
        protected override void OnActivated()
        {
            CreatePlugin();
        }

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

        private async void CameraPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == CameraTable.Defs.Columns.Name)
            {
                DisplayName = Camera.Name;
            }

            if (e.PropertyName == CameraTable.Defs.Columns.PluginId)
            {
                await DestroyPlugin();
                CreatePlugin();
            }
        }

        private async void CreatePlugin()
        {
            try
            {
                ErrorText = null;
                if (Camera.PluginId != PluginTable.Defs.Values.NullPluginId)
                {
                    Plugin = PluginFactory.Create(Camera);
                    Plugin.PluginException += PluginPluginException;
                    if (Plugin is ICameraInitialization init)
                    {
                        await init.InitializeCameraValuesAsync();
                        if (Plugin is ICameraColor color)
                        {
                            Brightness = color.Brightness;
                            Contrast = color.Contrast;
                            Hue = color.Hue;
                            Saturation = color.Saturation;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private void PluginPluginException(object sender, PluginException e)
        {
            ErrorText = e.Message;
        }

        private async Task DestroyPlugin()
        {
            if (Plugin != null)
            {
                await Plugin.StopVideoAsync();
                Plugin.PluginException -= PluginPluginException;
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