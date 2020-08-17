using Restless.Camera.Contracts;
using Restless.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Restless.Plugin.Amcrest
{
    [Export(typeof(ICameraPluginCreator))]
    public class AmcrestCgiCreator : PluginCreatorBase, ICameraPluginCreator
    {
        /// <summary>
        /// Gets the display name for the plugin.
        /// </summary>
        public string DisplayName => "Amcrest Camera Controller (CGI)";
        /// <summary>
        /// Gets the description for the plugin.
        /// </summary>
        public string Description => "Provides camera services for Amcrest IP cameras";
        /// <summary>
        /// Gets a unique id for the plugin.
        /// </summary>
        public string UniqueId => "A895E119-5DA7-4E2B-8F15-36C1ED1ED849";
        /// <summary>
        /// Creates an instance of the plugin.
        /// </summary>
        /// <param name="parms">The connection parameters</param>
        /// <returns>An instance of <see cref="AmcrestCgiController"/></returns>
        public ICameraPlugin Create(ConnectionParameters parms)
        {
            return new AmcrestCgiController(parms);
        }
    }

    public class AmcrestCgiController : RtspPluginBase, ICameraPlugin, ICameraMotion, ICameraSettings, ICameraColor, ICameraPreset, ICameraReboot
    {
        #region Private
        // start / stop
        private const string StartAction = "start";
        private const string StopAction = "stop";
        private const string MotionControlPath = "cgi-bin/ptz.cgi?action={0}&channel=1&code={1}&arg1={2}&arg2={3}&arg3=0";
        private const string ConfigGetControlPath = "cgi-bin/configManager.cgi?action=getConfig&name=";
        private const string ConfigSetControlPath = "cgi-bin/configManager.cgi?action=setConfig&";
        private const string RtspVideoStreamPath0 = "cam/realmonitor?channel=1&subtype=0";
        private const string RtspVideoStreamPath1 = "cam/realmonitor?channel=1&subtype=1";
        private const string MjpegVideoStreamPath = "cgi-bin/mjpg/video.cgi?channel=1&subtype=1";
        private readonly Dictionary<CameraMotion, Tuple<string,string>> motionMap;
        private readonly Dictionary<SettingItem, string> configMap;
        private readonly Dictionary<SettingItem, int> colorValueMap;
        private int videoStreamIndex;
        private int motionSpeed;
        #endregion

        /************************************************************************/

        #region Constructor (internal)
        internal AmcrestCgiController(ConnectionParameters parms) : base(parms)
        {
            VideoStreams.Add(new VideoStreamDescriptor(TransportProtocol.Rtsp, RtspVideoStreamPath0, "Rtsp High Res"));
            VideoStreams.Add(new VideoStreamDescriptor(TransportProtocol.Rtsp, RtspVideoStreamPath1, "Rtsp Low Res (640x480)"));
            VideoStreams.Add(new VideoStreamDescriptor(TransportProtocol.Http, MjpegVideoStreamPath, "Mjpeg (640x480)"));

            motionMap = new Dictionary<CameraMotion, Tuple<string, string>>()
            {
                // Note: CameraMotion.Stop requires a valid direction, but it stops for any direction.
                { CameraMotion.Stop, new Tuple<string, string>(StopAction, "Left") },
                { CameraMotion.Left, new Tuple<string, string>(StartAction, "Left") },
                { CameraMotion.Right,  new Tuple<string, string>(StartAction, "Right") },
                { CameraMotion.Up, new Tuple<string, string>(StartAction, "Up") },
                { CameraMotion.Down, new Tuple<string, string>(StartAction, "Down") },
            };

            // Motion supports speed of 1 - 8 
            // MotionSpeed is treated as a percentage, gets translated.
            MotionSpeed = 50;
            VideoStreamIndex = 0;

            colorValueMap = new Dictionary<SettingItem, int>()
            {
                { SettingItem.Brightness, 50 },
                { SettingItem.Contrast, 50 },
                { SettingItem.Hue, 50 },
                { SettingItem.Saturation, 50 },
            };

            configMap = new Dictionary<SettingItem, string>()
            {
                { SettingItem.Brightness, "VideoColor[0][0].Brightness=" },
                { SettingItem.Contrast, "VideoColor[0][0].Contrast=" },
                { SettingItem.Hue, "VideoColor[0][0].Hue=" },
                { SettingItem.Saturation, "VideoColor[0][0].Saturation=" },
                { SettingItem.FlipOn, "VideoInOptions[0].Flip=true" },
                { SettingItem.FlipOff, "VideoInOptions[0].Flip=false" },
                { SettingItem.MirrorOn, "VideoInOptions[0].Mirror=true" },
                { SettingItem.MirrorOff, "VideoInOptions[0].Mirror=false" },
            };
        }
        #endregion

        /************************************************************************/

        #region ICameraPlugin
        /// <summary>
        /// Gets or sets the index into video streams.
        /// </summary>
        public override int VideoStreamIndex
        {
            get => videoStreamIndex;
            set => videoStreamIndex = Math.Min(Math.Max(0, value), 2);
        }
        #endregion

        /************************************************************************/

        #region ICameraMotion
        /// <summary>
        /// Gets or sets the motion speed. Clamped between <see cref="MinSpeed"/> and <see cref="MaxSpeed"/>.
        /// </summary>
        public int MotionSpeed
        {
            get => motionSpeed;
            set => motionSpeed = GetTranslatedMotionSpeed(value);
        }

        /// <summary>
        /// Moves the camera according to the specified motion.
        /// </summary>
        /// <param name="motion">The motion.</param>
        public async void Move(CameraMotion motion)
        {
            if (motionMap.ContainsKey(motion))
            {
                await PerformClientGetAsync(GetCameraMotionUri(motion));
            }
        }
        #endregion

        /************************************************************************/

        #region ICameraPreset
        /// <summary>
        /// Gets the maximum value to be used for establishing or moving to a preset.
        /// </summary>
        public int MaxPreset => 25;

        /// <summary>
        /// Moves the camera to the specified preset position. Value is between 1 and <see cref="PresetMax"/>, inclusive.
        /// </summary>
        /// <param name="preset">The preset.</param>
        public async void MoveToPreset(int preset)
        {
            string uri = GetCameraMotionUri(StartAction, "GotoPreset", 0, preset); 
            await PerformClientGetAsync(uri);
        }

        /// <summary>
        /// Sets the specified preset.
        /// </summary>
        /// <param name="preset">The preset number to set.</param>
        public async void SetPreset(int preset)
        {
            string uri = GetCameraMotionUri(StartAction, "SetPreset", 0, preset);
            await PerformClientGetAsync(uri);
        }

        /// <summary>
        /// Clears the specified preset.
        /// </summary>
        /// <param name="preset">The preset number.</param>
        public async void ClearPreset(int preset)
        {
            string uri = GetCameraMotionUri(StartAction, "ClearPreset", 0, preset);
            await PerformClientGetAsync(uri);
        }
        #endregion

        /************************************************************************/

        #region ICameraColor
        /// <summary>
        /// Gets or sets the brightness.
        /// </summary>
        public int Brightness 
        {
            get => colorValueMap[SettingItem.Brightness];
            set => SetColorValue(SettingItem.Brightness, value);
        }

        /// <summary>
        /// Gets or sets the contrast.
        /// </summary>
        public int Contrast
        { 
            get => colorValueMap[SettingItem.Contrast];
            set => SetColorValue(SettingItem.Contrast, value);
        }

        /// <summary>
        /// Gets or sets the hue.
        /// </summary>
        public int Hue 
        {
            get => colorValueMap[SettingItem.Hue];
            set => SetColorValue(SettingItem.Hue, value);
        }

        /// <summary>
        /// Gets or sets the saturation.
        /// </summary>
        public int Saturation
        {
            get => colorValueMap[SettingItem.Saturation];
            set => SetColorValue(SettingItem.Saturation, value);
        }
        #endregion

        /************************************************************************/

        #region ICameraInitialization
        /// <summary>
        /// Initializes the camera values (brightenss, contrast, etc) by obtaining them from the camera.
        /// </summary>
        public async Task InitializeCameraValuesAsync()
        {
            await InitializeCameraValuesAsyncPrivate();
        }
        #endregion

        /************************************************************************/

        #region ICameraSettings
        /// <summary>
        /// Sets video flip
        /// </summary>
        /// <param name="value">true to flip video; otherwise, false.</param>
        public virtual async void SetFlip(bool value)
        {
            SettingItem item = value ? SettingItem.FlipOn : SettingItem.FlipOff;
            await PerformClientGetAsync(GetConfigurationUri(item));
        }

        /// <summary>
        /// Sets video mirror.
        /// </summary>
        /// <param name="value">true to mirror video; otherwise false.</param>
        public virtual async void SetMirror(bool value)
        {
            SettingItem item = value ? SettingItem.MirrorOn : SettingItem.MirrorOff;
            await PerformClientGetAsync(GetConfigurationUri(item));
        }
        #endregion

        /************************************************************************/

        #region ICameraReboot
        /// <summary>
        /// Reboots the camera.
        /// </summary>
        public async void Reboot()
        {
            await PerformClientGetAsync($"{GetDeviceRoot(TransportProtocol.Http)}/cgi-bin/magicBox.cgi?action=reboot");
        }
        #endregion

        /************************************************************************/

        #region Private methods
        /// <summary>
        /// Gets the translated motion speed.
        /// </summary>
        /// <param name="value">The value from the client. Can be 1-100</param>
        /// <returns>The translated speed, in this case 1-8</returns>
        private int GetTranslatedMotionSpeed(int value)
        {
            value = Math.Min(Math.Max(value, 1), 100);
            double newValue = Math.Round((value / 100.0) * 8.0, 0);
            return Math.Max((int)newValue, 1);
        }

        private async void SetColorValue(SettingItem item, int value)
        {
            if (colorValueMap[item] != value)
            {
                string uri = $"{GetConfigurationUri(item)}{value}";
                await PerformClientGetAsync(uri);
                colorValueMap[item] = value;
            }
        }

        private async Task InitializeCameraValuesAsyncPrivate()
        {
            string uri = $"{GetDeviceRoot(TransportProtocol.Http)}/{ConfigGetControlPath}VideoColor";
            string body = await PerformClientGetAsync(uri);
            if (!string.IsNullOrEmpty(body))
            {
                string[] lines = body.Split(Environment.NewLine);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        if (parts[0].EndsWith("[0].Brightness"))
                        {
                            colorValueMap[SettingItem.Brightness] = GetIntegerValue(parts[1]);
                        }

                        if (parts[0].EndsWith("[0].Contrast"))
                        {
                            colorValueMap[SettingItem.Contrast] = GetIntegerValue(parts[1]);
                        }

                        if (parts[0].EndsWith("[0].Hue"))
                        {
                            colorValueMap[SettingItem.Hue] = GetIntegerValue(parts[1]);
                        }

                        if (parts[0].EndsWith("[0].Saturation"))
                        {
                            colorValueMap[SettingItem.Saturation] = GetIntegerValue(parts[1]);
                        }
                    }
                }
            }
        }

        private int GetIntegerValue(string input, int defaultValue = 50)
        {
            if (int.TryParse(input, out int result)) return result;
            return defaultValue;
        }

        private string GetConfigurationUri(SettingItem item)
        {
            string mapData = string.Empty;
            if (configMap.ContainsKey(item)) mapData = configMap[item];

            return $"{GetDeviceRoot(TransportProtocol.Http)}/{ConfigSetControlPath}{mapData}";
        }

        private string GetCameraMotionUri(CameraMotion motion)
        {
            return GetCameraMotionUri(motionMap[motion].Item1, motionMap[motion].Item2, 0, MotionSpeed);
        }

        private string GetCameraMotionUri(string action, string code, int arg1, int arg2)
        {
            // MotionControlPath = "cgi-bin/ptz.cgi?action={0}&channel=1&code={1}&arg1={2}&arg2={3}&arg3=0";
            return $"{GetDeviceRoot(TransportProtocol.Http)}/{string.Format(MotionControlPath, action, code, arg1, arg2)}";
        }
        #endregion
    }
}