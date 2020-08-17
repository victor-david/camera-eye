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

    public class FoscamSdController : MjpegPluginBase, ICameraPlugin, ICameraMotion, ICameraSettings, ICameraPreset, ICameraReboot
    {
        #region Private
        //private const string CameraParmsCgi = "get_camera_params.cgi";
        private const string MotionControlPath = "decoder_control.cgi?command=";
        private const string CameraControlPath = "camera_control.cgi?";
        private const string BasePathToVideoStream = "videostream.cgi";
        private readonly Dictionary<CameraMotion, int> motionMap;
        private readonly Dictionary<SettingItem, string> configMap;
        private readonly Dictionary<SettingItem, int> colorValueMap;
        private readonly Dictionary<SettingItem, int> colorMaxMap;
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

            colorValueMap = new Dictionary<SettingItem, int>()
            {
                { SettingItem.Brightness, 50 },
                { SettingItem.Contrast, 50 },
                { SettingItem.Hue, 50 },
                { SettingItem.Saturation, 50 },
            };

            colorMaxMap = new Dictionary<SettingItem, int>()
            {
                { SettingItem.Brightness, 255 },
                { SettingItem.Contrast, 6 },
            };

            configMap = new Dictionary<SettingItem, string>()
            {
                { SettingItem.Brightness, "param=1&value=" },
                { SettingItem.Contrast, "param=2&value=" },
                { SettingItem.Flip, "param=5&value=" },
                { SettingItem.Mirror, "param=5&value=" },
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
        public SettingItem Supported => SettingItem.Brightness | SettingItem.Contrast | SettingItem.Flip | SettingItem.Mirror;

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
        /// Hue. Not supported.
        /// </summary>
        public int Hue
        {
            get => throw new NotSupportedException(nameof(Hue));
            set => throw new NotSupportedException(nameof(Hue));
        }

        /// <summary>
        /// Saturation. Not supported.
        /// </summary>
        public int Saturation
        {
            get => throw new NotSupportedException(nameof(Saturation));
            set => throw new NotSupportedException(nameof(Saturation));
        }

        /// <summary>
        /// Sets whether or not the video is flipped.
        /// </summary>
        /// <param name="value">true to flip video; otherwise, false.</param>
        public async void SetIsFlipped(bool value)
        {
            flipMirrorValue = (value) ? flipMirrorValue | 1 : flipMirrorValue & ~1;
            string uri = GetConfigurationUri(SettingItem.Flip, flipMirrorValue);
            await PerformClientGetAsync(uri);
        }

        /// <summary>
        /// Sets whether or not the video is mirrored.
        /// </summary>
        /// <param name="value">true to mirror video; otherwise false.</param>
        public async void SetIsMirrored(bool value)
        {
            flipMirrorValue = (value) ? flipMirrorValue | 2 : flipMirrorValue & ~2;
            string uri = GetConfigurationUri(SettingItem.Mirror, flipMirrorValue);
            await PerformClientGetAsync(uri);
        }

        /// <summary>
        /// Sets the rotation of the video. Not supported.
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
        /// Reboots the camera
        /// </summary>
        public async void Reboot()
        {
            await PerformClientGetAsync($"{GetDeviceRoot(TransportProtocol.Http)}/reboot.cgi");
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private async void SetColorValue(SettingItem item, int value)
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
                            colorValueMap[SettingItem.Brightness] = ToScaleZeroHundredColorValue(SettingItem.Brightness, GetIntegerValue(parts[1], 128));
                        }

                        if (parts[0].EndsWith("contrast"))
                        {
                            colorValueMap[SettingItem.Contrast] = ToScaleZeroHundredColorValue(SettingItem.Contrast, GetIntegerValue(parts[1], 4));
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


        private int ToScaleZeroHundredColorValue(SettingItem item, int nativeValue)
        {
            double pc = nativeValue / (double)colorMaxMap[item];
            return (int)(pc * 100.0);
        }

        private int ToNativeColorValue(SettingItem item, int scaleValue)
        {
            double pc = scaleValue / 100.0;
            return (int)(colorMaxMap[item] * pc);
        }

        //private string GetConfigurationFlipMirrorUri(SettingItem op, bool value)
        //{
        //    // 1 = flip, 2 = mirror, 3 = both
        //    if (op == SettingItem.FlipOn) flipMirrorValue |= 1;
        //    if (op == SettingItem.FlipOff) flipMirrorValue &= ~1;
        //    if (op == SettingItem.MirrorOn) flipMirrorValue |= 2;
        //    if (op == SettingItem.MirrorOff) flipMirrorValue &= ~2;
        //    return GetConfigurationUri(op, flipMirrorValue);
        //}

        private string GetConfigurationUri(SettingItem item, int value)
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