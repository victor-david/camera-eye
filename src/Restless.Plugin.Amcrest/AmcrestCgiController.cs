using Restless.Camera.Contracts;
using Restless.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

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
        /// <returns>An instance of <see cref="FoscamSdController"/></returns>
        public ICameraPlugin Create(ConnectionParameters parms)
        {
            return new AmcrestCgiController(parms);
        }
    }

    public class AmcrestCgiController : RtspPluginBase, ICameraPlugin, ICameraMotion, ICameraSettings, ICameraColor
    {
        #region Private
        // start / stop
        private const string StartAction = "start";
        private const string StopAction = "stop";
        private const string MotionControlPath = "cgi-bin/ptz.cgi?action={0}&channel=1&code={1}&arg1=0&arg2={2}&arg3=0";
        private const string ConfigGetControlPath = "cgi-bin/configManager.cgi?action=getConfig&name=";
        private const string ConfigSetControlPath = "cgi-bin/configManager.cgi?action=setConfig&";
        private const string RtspVideoStreamPath0 = "cam/realmonitor?channel=1&subtype=0";
        private const string RtspVideoStreamPath1 = "cam/realmonitor?channel=1&subtype=1";
        private const string MjpegVideoStreamPath = "cgi-bin/mjpg/video.cgi?channel=1&subtype=1";
        private readonly Dictionary<CameraMotion, Tuple<string,string>> motionMap;
        private readonly Dictionary<ConfigItem, string> configMap;
        private readonly Dictionary<ConfigItem, int> colorValueMap;
        private int videoStreamIndex;
        private int motionSpeed;

        private enum ConfigItem
        {
            Brightness,Contrast,
            Hue,Saturation,
            FlipOn, FlipOff,
            MirrorOn, MirrorOff,
            InfraRedOn, InfraRedOff,
        }
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
            // Supports 1 - 8
            MotionSpeed = 5;
            VideoStreamIndex = 0;

            colorValueMap = new Dictionary<ConfigItem, int>()
            {
                { ConfigItem.Brightness, -1 },
                { ConfigItem.Contrast, -1 },
                { ConfigItem.Hue, -1 },
                { ConfigItem.Saturation, -1 },
            };

            configMap = new Dictionary<ConfigItem, string>()
            {
                { ConfigItem.Brightness, "VideoColor[0][0].Brightness={0}" },
                { ConfigItem.Contrast, "VideoColor[0][0].Contrast={0}" },
                { ConfigItem.Hue, "VideoColor[0][0].Hue={0}" },
                { ConfigItem.Saturation, "VideoColor[0][0].Saturation={0}" },
                { ConfigItem.FlipOn, "VideoInOptions[0].Flip=true" },
                { ConfigItem.FlipOff, "VideoInOptions[0].Flip=false" },
                { ConfigItem.MirrorOn, "VideoInOptions[0].Mirror=true" },
                { ConfigItem.MirrorOff, "VideoInOptions[0].Mirror=false" },
                { ConfigItem.InfraRedOn, "VideoInOptions[0].InfraRed=true&VideoInOptions[0].InfraRedLevel=50" },
                { ConfigItem.InfraRedOff, "VideoInOptions[0].InfraRed=false" },
            };

            InitializeColorValues();
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
        /// Gets the minimum supported motion speed.
        /// </summary>
        public int MinSpeed => 1;

        /// <summary>
        /// Gets the maximum supported motion speed.
        /// </summary>
        public int MaxSpeed => 8;

        /// <summary>
        /// Gets or sets the motion speed. Clamped between <see cref="MinSpeed"/> and <see cref="MaxSpeed"/>.
        /// </summary>
        public int MotionSpeed
        {
            get => motionSpeed;
            set => motionSpeed = Math.Min(Math.Max(MinSpeed, value), MaxSpeed);
        }

        public async void Move(CameraMotion motion)
        {
            if (motionMap.ContainsKey(motion))
            {
                await PerformClientRequest(GetCameraMotionUri(motion));
            }
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
        /// Gets or sets the hue.
        /// </summary>
        public int Hue 
        {
            get => colorValueMap[ConfigItem.Hue];
            set => SetColorValue(ConfigItem.Hue, value);
        }

        /// <summary>
        /// Gets or sets the saturation.
        /// </summary>
        public int Saturation
        {
            get => colorValueMap[ConfigItem.Saturation];
            set => SetColorValue(ConfigItem.Saturation, value);
        }

        /// <summary>
        /// Occurs when the color values (brightness, contrast, etc.) have been retrieved from the camera.
        /// </summary>
        public event EventHandler ColorValuesInitialized;
        #endregion

        /************************************************************************/

        #region ICameraSettings
        /// <summary>
        /// Sets video flip
        /// </summary>
        /// <param name="value">true to flip video; otherwise, false.</param>
        public async void SetFlip(bool value)
        {
            ConfigItem item = value ? ConfigItem.FlipOn : ConfigItem.FlipOff;
            await PerformClientRequest(GetConfigurationUri(item));
        }

        /// <summary>
        /// Sets video mirror.
        /// </summary>
        /// <param name="value">true to mirror video; otherwise false.</param>
        public async void SetMirror(bool value)
        {
            ConfigItem item = value ? ConfigItem.MirrorOn : ConfigItem.MirrorOff;
            await PerformClientRequest(GetConfigurationUri(item));
        }

        /// <summary>
        /// Sets infra red.
        /// </summary>
        /// <param name="value">true to turn on infra red; false to turn it off.</param>
        public async void SetInfraRed(bool value)
        {
            ConfigItem item = value ? ConfigItem.InfraRedOn : ConfigItem.InfraRedOff;
            await PerformClientRequest(GetConfigurationUri(item));
        }
        #endregion

        /************************************************************************/

        #region Private methods

        private async void SetColorValue(ConfigItem item, int value)
        {
            if (colorValueMap[item] != value)
            {
                string uri = GetConfigurationUri(item);
                uri = string.Format(uri, value);
                await PerformClientRequest(uri);
                colorValueMap[item] = value;
            }
        }

        private async void InitializeColorValues()
        {
            string uri = $"{GetDeviceRoot(TransportProtocol.Http)}/{ConfigGetControlPath}VideoColor";
            string body = await PerformClientRequest(uri);
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
                            colorValueMap[ConfigItem.Brightness] = GetIntegerValue(parts[1]);
                        }

                        if (parts[0].EndsWith("[0].Contrast"))
                        {
                            colorValueMap[ConfigItem.Contrast] = GetIntegerValue(parts[1]);
                        }

                        if (parts[0].EndsWith("[0].Hue"))
                        {
                            colorValueMap[ConfigItem.Hue] = GetIntegerValue(parts[1]);
                        }

                        if (parts[0].EndsWith("[0].Saturation"))
                        {
                            colorValueMap[ConfigItem.Saturation] = GetIntegerValue(parts[1]);
                        }
                    }
                }
            }
            EnsureColorValuesInitialized();
            ColorValuesInitialized?.Invoke(this, EventArgs.Empty);
        }

        private void EnsureColorValuesInitialized()
        {
            foreach (ConfigItem key in colorValueMap.Keys)
            {
                if (colorValueMap[key] == -1) colorValueMap[key] = 50;
            }
        }

        private int GetIntegerValue(string input, int defaultValue = 50)
        {
            if (int.TryParse(input, out int result)) return result;
            return defaultValue;
        }


        private string GetConfigurationUri(ConfigItem item)
        {
            string mapData = string.Empty;
            if (configMap.ContainsKey(item)) mapData = configMap[item];

            return $"{GetDeviceRoot(TransportProtocol.Http)}/{ConfigSetControlPath}{mapData}";
        }

        private string GetCameraMotionUri(CameraMotion motion)
        {
            return $"{GetDeviceRoot(TransportProtocol.Http)}/{string.Format(MotionControlPath, motionMap[motion].Item1, motionMap[motion].Item2, MotionSpeed)}";
        }
        #endregion
    }
}
