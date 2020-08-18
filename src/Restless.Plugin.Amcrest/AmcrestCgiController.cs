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

    public class AmcrestCgiController : RtspPluginBase, ICameraPlugin, ICameraMotion, ICameraSettings, ICameraPreset, ICameraReboot
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
        private readonly Dictionary<CameraSetting, string> configMap;
        private readonly Dictionary<CameraSetting, int> colorValueMap;
        private int videoStreamIndex;
        private int motionSpeed;
        private int translatedMotionSpeed;
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

            colorValueMap = new Dictionary<CameraSetting, int>()
            {
                { CameraSetting.Brightness, 50 },
                { CameraSetting.Contrast, 50 },
                { CameraSetting.Hue, 50 },
                { CameraSetting.Saturation, 50 },
            };

            configMap = new Dictionary<CameraSetting, string>()
            {
                { CameraSetting.Brightness, "VideoColor[0][0].Brightness=" },
                { CameraSetting.Contrast, "VideoColor[0][0].Contrast=" },
                { CameraSetting.Hue, "VideoColor[0][0].Hue=" },
                { CameraSetting.Saturation, "VideoColor[0][0].Saturation=" },
                { CameraSetting.Flip, "VideoInOptions[0].Flip=" },
                { CameraSetting.Mirror, "VideoInOptions[0].Mirror=" },
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
        /// Gets or sets the motion speed.
        /// </summary>
        public int MotionSpeed
        {
            get => motionSpeed;
            set => SetMotionSpeed(value);
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
        /// Gets a bitwise combination value that describes which setting items are supported.
        /// </summary>
        public CameraSetting Supported => CameraSetting.Brightness | CameraSetting.Contrast | CameraSetting.Hue | CameraSetting.Saturation | CameraSetting.Flip | CameraSetting.Mirror;

        /// <summary>
        /// Gets or sets the brightness.
        /// </summary>
        public int Brightness
        {
            get => colorValueMap[CameraSetting.Brightness];
            set => SetColorValue(CameraSetting.Brightness, value);
        }

        /// <summary>
        /// Gets or sets the contrast.
        /// </summary>
        public int Contrast
        {
            get => colorValueMap[CameraSetting.Contrast];
            set => SetColorValue(CameraSetting.Contrast, value);
        }

        /// <summary>
        /// Gets or sets the hue.
        /// </summary>
        public int Hue
        {
            get => colorValueMap[CameraSetting.Hue];
            set => SetColorValue(CameraSetting.Hue, value);
        }

        /// <summary>
        /// Gets or sets the saturation.
        /// </summary>
        public int Saturation
        {
            get => colorValueMap[CameraSetting.Saturation];
            set => SetColorValue(CameraSetting.Saturation, value);
        }

        /// <summary>
        /// Sets whether or not the video is flipped.
        /// </summary>
        /// <param name="value">true to flip video; otherwise, false.</param>
        public async void SetIsFlipped(bool value)
        {
            string uri = GetConfigurationUri(CameraSetting.Flip, value.ToString().ToLower());
            await PerformClientGetAsync(uri);
        }

        /// <summary>
        /// Sets whether or not the video is mirrored.
        /// </summary>
        /// <param name="value">true to mirror video; otherwise false.</param>
        public async void SetIsMirrored(bool value)
        {
            string uri = GetConfigurationUri(CameraSetting.Mirror, value.ToString().ToLower());
            await PerformClientGetAsync(uri);
        }

        /// <summary>
        /// Sets the rotation of the video.
        /// </summary>
        /// <param name="value">The rotation.</param>
        public void SetRotation(Rotation value)
        {
            throw new NotSupportedException();
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
        /// Sets the motion speed. 
        /// </summary>
        /// <param name="value">The value from the client. 0-100</param>
        /// <remarks>
        /// This method saves the client speed (0-100) and calculates translatedMotionSpeed (1-8)
        /// which is used when moving the camera. Motion speed is not saved in the camera; it's
        /// a parameter in the url that moves the camera.
        /// </remarks>
        private void SetMotionSpeed(int value)
        {
            motionSpeed = value;
            value = Math.Min(Math.Max(value, 1), 100);
            double newValue = Math.Round((value / 100.0) * 8.0, 0);
            translatedMotionSpeed = Math.Max((int)newValue, 1);
        }

        private async void SetColorValue(CameraSetting item, int value)
        {
            if (colorValueMap[item] != value)
            {
                string uri = GetConfigurationUri(item, value);
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
                            colorValueMap[CameraSetting.Brightness] = GetIntegerValue(parts[1]);
                        }

                        if (parts[0].EndsWith("[0].Contrast"))
                        {
                            colorValueMap[CameraSetting.Contrast] = GetIntegerValue(parts[1]);
                        }

                        if (parts[0].EndsWith("[0].Hue"))
                        {
                            colorValueMap[CameraSetting.Hue] = GetIntegerValue(parts[1]);
                        }

                        if (parts[0].EndsWith("[0].Saturation"))
                        {
                            colorValueMap[CameraSetting.Saturation] = GetIntegerValue(parts[1]);
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

        private string GetConfigurationUri(CameraSetting item, object value)
        {
            string mapData = string.Empty;
            if (configMap.ContainsKey(item)) mapData = configMap[item];

            return $"{GetDeviceRoot(TransportProtocol.Http)}/{ConfigSetControlPath}{mapData}{value}";
        }

        private string GetCameraMotionUri(CameraMotion motion)
        {
            return GetCameraMotionUri(motionMap[motion].Item1, motionMap[motion].Item2, 0, translatedMotionSpeed);
        }

        private string GetCameraMotionUri(string action, string code, int arg1, int arg2)
        {
            // MotionControlPath = "cgi-bin/ptz.cgi?action={0}&channel=1&code={1}&arg1={2}&arg2={3}&arg3=0";
            return $"{GetDeviceRoot(TransportProtocol.Http)}/{string.Format(MotionControlPath, action, code, arg1, arg2)}";
        }
        #endregion
    }
}