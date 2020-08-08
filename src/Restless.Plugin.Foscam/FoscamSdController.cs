using Restless.Camera.Contracts;
using Restless.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

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

    public class FoscamSdController : HttpPluginBase, ICameraPlugin, ICameraMotion, ICameraReset
    {
        #region Private
        //private const string CameraParmsCgi = "get_camera_params.cgi";
        private const string MotionControlCgi = "decoder_control.cgi?command={0}";
        //private const string CameraControlCgi = "camera_control.cgi?param={0}&value={1}";
        private const string BasePathToVideoStream = "videostream.cgi";
        private readonly Dictionary<CameraMotion, int> motionMap;
        private int videoStreamIndex;
        private int motionSpeed;
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
        /// Gets the minimum supported motion speed.
        /// </summary>
        public int MinSpeed => 0;

        /// <summary>
        /// Gets the maximum supported motion speed.
        /// </summary>
        public int MaxSpeed => 15;

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
                await PerformClientGetAsync(GetCameraMotionUri(motion));
            }
        }
        #endregion

        /************************************************************************/

        #region ICameraReset
        public void Reset()
        {
        }

        public async void Reboot()
        {
            await PerformClientGetAsync($"{GetDeviceRoot(TransportProtocol.Http)}/reboot.cgi");
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private string GetCameraMotionUri(CameraMotion motion)
        {
            return $"{GetDeviceRoot(TransportProtocol.Http)}/{string.Format(MotionControlCgi, motionMap[motion])}";
        }
        #endregion
    }
}