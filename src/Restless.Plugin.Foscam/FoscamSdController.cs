﻿using Restless.Camera.Contracts;
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
        private readonly Dictionary<CameraSetting, string> configMap;
        private readonly Dictionary<CameraSetting, int> colorValueMap;
        private readonly Dictionary<CameraSetting, int> colorMaxMap;
        private int videoStreamIndex;
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
            MotionSpeed = 50;

            colorValueMap = new Dictionary<CameraSetting, int>()
            {
                { CameraSetting.Brightness, 50 },
                { CameraSetting.Contrast, 50 },
                { CameraSetting.Hue, 50 },
                { CameraSetting.Saturation, 50 },
            };

            colorMaxMap = new Dictionary<CameraSetting, int>()
            {
                { CameraSetting.Brightness, 255 },
                { CameraSetting.Contrast, 6 },
            };

            configMap = new Dictionary<CameraSetting, string>()
            {
                { CameraSetting.Brightness, "param=1&value=" },
                { CameraSetting.Contrast, "param=2&value=" },
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
        /// Gets or sets the motion speed.
        /// </summary>
        public int MotionSpeed { get; private set; }

        /// <summary>
        /// Sets the motion speed.
        /// </summary>
        /// <param name="value">The value (0-100)</param>
        /// <returns>true if speed set successfully; otherwise, false.</returns>
        /// <remarks>
        /// This plugin needs to set the speed within the camera and might not be able to do so
        /// due to authentication or network problem.
        /// </remarks>
        public async Task<bool> SetMotionSpeedAsync(int value)
        {
            if (value != MotionSpeed)
            {
                return await SetMotionSpeed(value);
            }
            return false;
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
        public CameraSetting Supported => CameraSetting.Brightness | CameraSetting.Contrast; 

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
        /// <summary>
        /// Sets the motion speed
        /// </summary>
        /// <param name="value">The value from the client. 0-100</param>
        /// <remarks>
        /// This method attempts to set the translated motion speed (0-30) within the camera.
        /// If successful, it sets the current motion speed to the incoming value. Otherwise,
        /// the current motion speed is not changed. This method is only called when the
        /// incoming value is different than the current value. See <see cref="SetMotionSpeed(int)"/> above.
        /// </remarks>
        private async Task<bool> SetMotionSpeed(int value)
        {
            int clamped = Math.Min(Math.Max(value, 0), 100);
            int newValue = (int)Math.Round((clamped / 100.0) * 30.0, 0);
            // must be multiple of 2. And its inverted; lower value = faster)
            newValue = 30 - ((newValue % 2 != 0) ? newValue + 1 : newValue);
            string uri = $"{GetDeviceRoot(TransportProtocol.Http)}/set_misc.cgi?ptz_patrol_rate={newValue}";
            string body = await PerformClientGetAsync(uri);
            if (!string.IsNullOrEmpty(body))
            {
                MotionSpeed = value;
                return true;
            }
            return false;
        }

        private async void SetColorValue(CameraSetting item, int value)
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
                            colorValueMap[CameraSetting.Brightness] = ToScaleZeroHundredColorValue(CameraSetting.Brightness, GetIntegerValue(parts[1], 128));
                        }

                        if (parts[0].EndsWith("contrast"))
                        {
                            colorValueMap[CameraSetting.Contrast] = ToScaleZeroHundredColorValue(CameraSetting.Contrast, GetIntegerValue(parts[1], 4));
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

        private int ToScaleZeroHundredColorValue(CameraSetting item, int nativeValue)
        {
            double pc = nativeValue / (double)colorMaxMap[item];
            return (int)(pc * 100.0);
        }

        private int ToNativeColorValue(CameraSetting item, int scaleValue)
        {
            double pc = scaleValue / 100.0;
            return (int)(colorMaxMap[item] * pc);
        }

        private string GetConfigurationUri(CameraSetting item, int value)
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