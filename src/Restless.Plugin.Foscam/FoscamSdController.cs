using Restless.Camera.Contracts;
using Restless.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Restless.Plugin.Foscam
{
    [Export(typeof(ICameraPluginCreator))]
    public class FoscamSdCreator : PluginCreatorBase, ICameraPluginCreator
    {
        /// <summary>
        /// Gets the display name for the plugin.
        /// </summary>
        public string DisplayName => "Foscam SD Camera Controller";
        /// <summary>
        /// Gets the description for the plugin.
        /// </summary>
        public string Description => "Provides camera services for older Fosacam IP cameras";
        /// <summary>
        /// Gets a unique id for the plugin.
        /// </summary>
        public string UniqueId => "690B360C-4C69-4C00-AAC9-6FA1CE0BBBE3";
        /// <summary>
        /// Creates an instance of the plugin.
        /// </summary>
        /// <param name="parms">The connection parameters</param>
        /// <returns>An instance of <see cref="FoscamSdController"/></returns>
        public ICameraPlugin Create(ConnectionParameters parms)
        {
            return new FoscamSdController(parms);
        }
    }

    public class FoscamSdController : MjpegPluginBase, ICameraPlugin, ICameraMotion, ICameraSettings, ICameraColor, ICameraPreset, ICameraReboot
    {
        #region Private
        //private const string CameraParmsCgi = "get_camera_params.cgi";
        private const string MotionControlPath = "decoder_control.cgi?command=";
        private const string CameraControlPath = "camera_control.cgi?";
        private const string BasePathToVideoStream = "videostream.cgi";
        private readonly Dictionary<CameraMotion, int> motionMap;
        private readonly Dictionary<ConfigItem, string> configMap;
        private readonly Dictionary<ConfigItem, int> colorValueMap;
        private readonly Dictionary<ConfigItem, int> colorMaxMap;
        private int videoStreamIndex;
        // supports 0 (fast) to 15 (slow)
        private int motionSpeed;
        // 0 = normal, 1 = flip, 2 = mirror, 3 = both
        private int flipMirrorValue;
        #endregion

        /************************************************************************/

        #region Constructor (internal)
        internal FoscamSdController(ConnectionParameters parms) : base(parms)
        {
            VideoStreams.Add(new VideoStreamDescriptor(TransportProtocol.Http, $"{BasePathToVideoStream}?resolution=32", "MJPEG 640x480"));
            VideoStreams.Add(new VideoStreamDescriptor(TransportProtocol.Http, $"{BasePathToVideoStream}?resolution=8", "MJPEG 320x240"));

            motionMap = new Dictionary<CameraMotion, int>()
            {
                { CameraMotion.Stop, 1 },
                { CameraMotion.Left, 6 },
                { CameraMotion.Right, 4 },
                { CameraMotion.Up, 0 },
                { CameraMotion.Down, 2 },
                { CameraMotion.Center, 25 },
                { CameraMotion.PatrolVertical, 26 },
                { CameraMotion.PatrolHorizontal, 28 },
            };
            VideoStreamIndex = 0;

            colorValueMap = new Dictionary<ConfigItem, int>()
            {
                { ConfigItem.Brightness, 50 },
                { ConfigItem.Contrast, 50 },
                { ConfigItem.Hue, 50 },
                { ConfigItem.Saturation, 50 },
            };

            colorMaxMap = new Dictionary<ConfigItem, int>()
            {
                { ConfigItem.Brightness, 255 },
                { ConfigItem.Contrast, 6 },
            };

            configMap = new Dictionary<ConfigItem, string>()
            {
                { ConfigItem.Brightness, "param=1&value=" },
                { ConfigItem.Contrast, "param=2&value=" },
                { ConfigItem.FlipOn, "param=5&value=" },
                { ConfigItem.FlipOff, "param=5&value=" },
                { ConfigItem.MirrorOn, "param=5&value=" },
                { ConfigItem.MirrorOff, "param=5&value=" },
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
            set => videoStreamIndex = Math.Min(Math.Max(0, value), 1);
        }
        #endregion

        /************************************************************************/

        #region ICameraMotion
        /// <summary>
        /// Gets or sets the motion speed. (currently not used)
        /// </summary>
        public int MotionSpeed
        {
            get => motionSpeed;
            set => motionSpeed = value;
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
        public int MaxPreset => 32;

        /// <summary>
        /// Moves the camera to the specified preset position. Value is between 1 and <see cref="PresetMax"/>, inclusive.
        /// </summary>
        /// <param name="preset">The preset.</param>
        public async void MoveToPreset(int preset)
        {
            int code = 31 + ((preset - 1) * 2);
            string uri = GetCameraMotionUri(code);
            await PerformClientGetAsync(uri);
        }

        /// <summary>
        /// Sets the specified preset.
        /// </summary>
        /// <param name="preset">The preset number to set.</param>
        public async void SetPreset(int preset)
        {
            int code = 30 + ((preset - 1) * 2);
            string uri = GetCameraMotionUri(code);
            await PerformClientGetAsync(uri);
        }

        /// <summary>
        /// Clears the specified preset.
        /// </summary>
        /// <param name="preset">The preset number.</param>
        public void ClearPreset(int preset)
        {
            // Foscam doesn't seem to have a way to clear the preset.
        }
        #endregion

        /************************************************************************/

        #region ICameraColor
        /// <summary>
        /// Gets or sets the brightness.
        /// </summary>
        public int Brightness
        {
            get => colorValueMap[ConfigItem.Brightness];
            set => SetColorValue(ConfigItem.Brightness, value);
        }

        /// <summary>
        /// Gets or sets the contrast.
        /// </summary>
        public int Contrast
        {
            get => colorValueMap[ConfigItem.Contrast];
            set => SetColorValue(ConfigItem.Contrast, value);
        }

        /// <summary>
        /// Gets the hue.
        /// </summary>
        public int Hue
        {
            get => colorValueMap[ConfigItem.Hue];
            set { }
        }

        /// <summary>
        /// Gets or sets the saturation.
        /// </summary>
        public int Saturation
        {
            get => colorValueMap[ConfigItem.Saturation];
            set { }
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
        /// Sets video flip.
        /// </summary>
        /// <param name="value">true to flip video; otherwise, false.</param>
        public virtual async void SetFlip(bool value)
        {
            ConfigItem op = value ? ConfigItem.FlipOn : ConfigItem.FlipOff;
            await PerformClientGetAsync(GetConfigurationFlipMirrorUri(op));
        }

        /// <summary>
        /// Sets video mirror.
        /// </summary>
        /// <param name="value">true to mirror video; otherwise false.</param>
        public virtual async void SetMirror(bool value)
        {
            ConfigItem op = value ? ConfigItem.MirrorOn : ConfigItem.MirrorOff;
            await PerformClientGetAsync(GetConfigurationFlipMirrorUri(op));
        }
        #endregion

        /************************************************************************/

        #region ICameraReboot
        /// <summary>
        /// Reboots the camera
        /// </summary>
        public async void Reboot()
        {
            await PerformClientGetAsync($"{GetDeviceRoot(TransportProtocol.Http)}/reboot.cgi");
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private async void SetColorValue(ConfigItem item, int value)
        {
            if (colorValueMap[item] != value)
            {
                string uri = GetConfigurationUri(item,ToNativeColorValue(item, value));
                await PerformClientGetAsync(uri);
                colorValueMap[item] = value;
            }
        }
        private async Task InitializeCameraValuesAsyncPrivate()
        {
            string uri = $"{GetDeviceRoot(TransportProtocol.Http)}/get_camera_params.cgi";
            string body = await PerformClientGetAsync(uri);

            if (!string.IsNullOrEmpty(body))
            {
                string[] lines = body.Split(';');
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        if (parts[0].EndsWith("brightness"))
                        {
                            colorValueMap[ConfigItem.Brightness] = ToScaleZeroHundredColorValue(ConfigItem.Brightness, GetIntegerValue(parts[1], 128));
                        }

                        if (parts[0].EndsWith("contrast"))
                        {
                            colorValueMap[ConfigItem.Contrast] = ToScaleZeroHundredColorValue(ConfigItem.Contrast, GetIntegerValue(parts[1], 4));
                        }

                        if (parts[0].EndsWith("flip"))
                        {
                            flipMirrorValue = GetIntegerValue(parts[1], 0);
                        }
                    }
                }
            }
        }

        private int GetIntegerValue(string input, int defaultValue)
        {
            if (int.TryParse(input, out int result)) return result;
            return defaultValue;
        }


        private int ToScaleZeroHundredColorValue(ConfigItem item, int nativeValue)
        {
            double pc = nativeValue / (double)colorMaxMap[item];
            return (int)(pc * 100.0);
        }

        private int ToNativeColorValue(ConfigItem item, int scaleValue)
        {
            double pc = scaleValue / 100.0;
            return (int)(colorMaxMap[item] * pc);
        }

        private string GetConfigurationFlipMirrorUri(ConfigItem op)
        {
            // 1 = flip, 2 = mirror, 3 = both
            if (op == ConfigItem.FlipOn) flipMirrorValue |= 1;
            if (op == ConfigItem.FlipOff) flipMirrorValue &= ~1;
            if (op == ConfigItem.MirrorOn) flipMirrorValue |= 2;
            if (op == ConfigItem.MirrorOff) flipMirrorValue &= ~2;
            return  GetConfigurationUri(op, flipMirrorValue);
        }

        private string GetConfigurationUri(ConfigItem item, int value)
        {
            return $"{GetDeviceRoot(TransportProtocol.Http)}/{CameraControlPath}{configMap[item]}{value}";
        }

        private string GetCameraMotionUri(CameraMotion motion)
        {
            return GetCameraMotionUri(motionMap[motion]);
        }

        private string GetCameraMotionUri(int code)
        {
            // MotionControlPath = "decoder_control.cgi?command=";
            return $"{GetDeviceRoot(TransportProtocol.Http)}/{MotionControlPath}{code}";
        }
        #endregion
    }
}