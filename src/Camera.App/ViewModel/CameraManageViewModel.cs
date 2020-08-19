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
                OnPropertyChanged(nameof(PluginIsMotion));
                OnPropertyChanged(nameof(PluginIsSettings));
                OnPropertyChanged(nameof(PluginSupportsBrightness));
                OnPropertyChanged(nameof(PluginSupportsContrast));
                OnPropertyChanged(nameof(PluginSupportsHue));
                OnPropertyChanged(nameof(PluginSupportsSaturation));
                OnPropertyChanged(nameof(PluginSupportsFlip));
                OnPropertyChanged(nameof(PluginSupportsMirror));
                OnPropertyChanged(nameof(PluginSupportsRotation));
                OnPropertyChanged(nameof(PluginSupportsOrientation));
                OnPropertyChanged(nameof(PluginIsReboot));
                OnPropertyChanged(nameof(PluginIsNone));
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraMotion.
        /// </summary>
        public bool PluginIsMotion
        {
            get => plugin is ICameraMotion;
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraSettings.
        /// </summary>
        public bool PluginIsSettings
        {
            get => plugin is ICameraSettings;
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraSettings.Brightness
        /// </summary>
        public bool PluginSupportsBrightness
        {
            get => PluginSupportsSettings(CameraSetting.Brightness);
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraSettings.Contrast
        /// </summary>
        public bool PluginSupportsContrast
        {
            get => PluginSupportsSettings(CameraSetting.Contrast);
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraSettings.Hue
        /// </summary>
        public bool PluginSupportsHue
        {
            get => PluginSupportsSettings(CameraSetting.Hue);
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraSettings.Saturation
        /// </summary>
        public bool PluginSupportsSaturation
        {
            get => PluginSupportsSettings(CameraSetting.Saturation);
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraSettings.Flip
        /// </summary>
        public bool PluginSupportsFlip
        {
            get => PluginSupportsSettings(CameraSetting.Flip);
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraSettings.Flip
        /// </summary>
        public bool PluginSupportsMirror
        {
            get => PluginSupportsSettings(CameraSetting.Mirror);
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraSettings.Rotation
        /// </summary>
        public bool PluginSupportsRotation
        {
            get => PluginSupportsSettings(CameraSetting.Rotation);
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraSettings.Flip, ICameraSettings.Mirror, or ICameraSettings.Rotation.
        /// </summary>
        public bool PluginSupportsOrientation
        {
            get => PluginSupportsFlip || PluginSupportsMirror || PluginSupportsRotation;
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin supports ICameraReboot.
        /// </summary>
        public bool PluginIsReboot
        {
            get => plugin is ICameraReboot;
        }

        /// <summary>
        /// Gets a boolean value that indicates if the plugin does not support additional configuration.
        /// </summary>
        public bool PluginIsNone
        {
            get => !PluginIsMotion && !PluginIsSettings && !PluginIsReboot;
        }

        /// <summary>
        /// Gets or sets the motion speed.
        /// </summary>
        public int MotionSpeed
        {
            get => (int)Camera.MotionSpeed;
            set => SetMotionSpeed(value);
        }

        /// <summary>
        /// Gets or sets a boolean value that determines if camera motion is translated on the X axis.
        /// </summary>
        public bool TranslateX
        {
            get => Camera.Flags.HasFlag(CameraFlags.TranslateX);
            set
            {
                Camera.SetTranslateX(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that determines if camera motion is translated on the Y axis.
        /// </summary>
        public bool TranslateY
        {
            get => Camera.Flags.HasFlag(CameraFlags.TranslateY);
            set
            {
                Camera.SetTranslateY(value);
                OnPropertyChanged();
            }
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
                if (Plugin is ICameraSettings settings && settings.Supported.HasFlag(CameraSetting.Brightness))
                {
                    settings.Brightness = value;
                }
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
                if (Plugin is ICameraSettings settings && settings.Supported.HasFlag(CameraSetting.Contrast))
                {
                    settings.Contrast = value;
                }
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
                if (Plugin is ICameraSettings settings && settings.Supported.HasFlag(CameraSetting.Hue))
                {
                    settings.Hue = value;
                }
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
                if (Plugin is ICameraSettings settings && settings.Supported.HasFlag(CameraSetting.Saturation))
                {
                    settings.Saturation = value;
                }
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
        /// Gets an IEnumerable of of int for the presets
        /// </summary>
        public IEnumerable<int> PresetList
        {
            get => Enumerable.Range(1, CameraRow.MaxMaxPreset);
        }

        /// <summary>
        /// Gets or sets the selected max preset value.
        /// </summary>
        public int SelectedMaxPreset
        {
            get => (int)Camera.MaxPreset;
            set => Camera.MaxPreset = value;
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
            Commands.Add("FlipVideo", RelayCommand.Create(RunFlipVideoCommand));
            Commands.Add("MirrorVideo", RelayCommand.Create(RunMirrorVideoCommand));
            Commands.Add("RotateVideo", RelayCommand.Create(RunRotateVideoCommand));
            Commands.Add("Reboot", RelayCommand.Create(RunRebootCommand));

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
            Config.SaveManageWindow(WindowOwner);
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

        private async void SetMotionSpeed(int value)
        {
            if (Plugin is ICameraMotion motion)
            {
                int temp = (int)Camera.MotionSpeed;
                Camera.MotionSpeed = value;
                if (!await motion.SetMotionSpeedAsync(value))
                {
                    Camera.MotionSpeed = temp;
                }
                OnPropertyChanged(nameof(MotionSpeed));
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
                        if (Plugin is ICameraSettings settings)
                        {
                            if (settings.Supported.HasFlag(CameraSetting.Brightness)) Brightness = settings.Brightness;
                            if (settings.Supported.HasFlag(CameraSetting.Contrast)) Contrast = settings.Contrast;
                            if (settings.Supported.HasFlag(CameraSetting.Hue)) Hue = settings.Hue;
                            if (settings.Supported.HasFlag(CameraSetting.Saturation)) Saturation = settings.Saturation;
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

        private bool PluginSupportsSettings(CameraSetting item)
        {
            return plugin is ICameraSettings settings && settings.Supported.HasFlag(item);
        }

        private void RunFlipVideoCommand(object parm)
        {
            if (PluginSupportsSettings(CameraSetting.Flip) && parm is bool value)
            {
                RunPluginMethod((plugin as ICameraSettings).SetIsFlipped, value);
            }
        }

        private void RunMirrorVideoCommand(object parm)
        {
            if (PluginSupportsSettings(CameraSetting.Mirror) && parm is bool value)
            {
                RunPluginMethod((plugin as ICameraSettings).SetIsMirrored, value);
            }
        }

        private void RunRotateVideoCommand(object parm)
        {
            if (PluginSupportsSettings(CameraSetting.Rotation) && parm is Rotation value)
            {
                RunPluginMethod((plugin as ICameraSettings).SetRotation, value);
            }
        }

        private void RunPluginMethod<T>(Action<T> method, T value)
        {
            try
            {
                method(value);
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private void RunRebootCommand(object parm)
        {
            (plugin as ICameraReboot)?.Reboot();
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