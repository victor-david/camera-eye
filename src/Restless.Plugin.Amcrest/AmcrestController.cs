using Restless.Camera.Contracts;
using Restless.Plugin.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Restless.Plugin.Amcrest
{
    [Export(typeof(ICameraPluginCreator))]
    public class AmcrestCreator : PluginCreatorBase, ICameraPluginCreator
    {
        /// <summary>
        /// Gets the display name for the plugin.
        /// </summary>
        public string DisplayName => "Amcrest Camera Controller";
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
            return new AmcrestController(parms);
        }

    }

    public class AmcrestController : RtspPluginBase, ICameraPlugin, ICameraMotion
    {
        #region Private
        // start / stop
        private const string StartAction = "start";
        private const string StopAction = "stop";
        private const string MotionControlCgi = "cgi-bin/ptz.cgi?action={0}&channel=1&code={1}&arg1=0&arg2={2}&arg3=0";
        private const string RtspVideoStreamPath0 = "cam/realmonitor?channel=1&subtype=0";
        private const string RtspVideoStreamPath1 = "cam/realmonitor?channel=1&subtype=1";
        private const string MjpegVideoStreamPath = "cgi-bin/mjpg/video.cgi?channel=1&subtype=1";
        private readonly Dictionary<CameraMotion, Tuple<string,string>> motionMap;
        private int videoStreamIndex;
        private int motionSpeed;
        #endregion

        /************************************************************************/

        #region Constructor (internal)
        internal AmcrestController(ConnectionParameters parms) : base(parms)
        {
            VideoStreams.Add(new VideoStreamDescriptor(TransportProtocol.Rtsp, RtspVideoStreamPath0, "Rtsp High Res"));
            VideoStreams.Add(new VideoStreamDescriptor(TransportProtocol.Rtsp, RtspVideoStreamPath1, "Rtsp Low Res (640x480)"));
            VideoStreams.Add(new VideoStreamDescriptor(TransportProtocol.Http, MjpegVideoStreamPath, "Mjpeg (640x480)"));

            motionMap = new Dictionary<CameraMotion, Tuple<string,string>>()
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
                await PerformClientRequest(GetCameraMotionUri(GetAdjustedMotion(motion)));
            }
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private string GetCameraMotionUri(CameraMotion motion)
        {
            return $"{GetDeviceRoot(TransportProtocol.Http)}/{string.Format(MotionControlCgi, motionMap[motion].Item1, motionMap[motion].Item2, MotionSpeed)}";
        }

        private CameraMotion GetAdjustedMotion(CameraMotion motion)
        {
            if (Orientation.HasFlag(Orientation.Mirror))
            {
                if (motion == CameraMotion.Left) return CameraMotion.Right;
                if (motion == CameraMotion.Right) return CameraMotion.Left;
            }

            if (Orientation.HasFlag(Orientation.Flip))
            {
                if (motion == CameraMotion.Up) return CameraMotion.Down;
                if (motion == CameraMotion.Down) return CameraMotion.Up;
            }

            return motion;
        }
        #endregion
    }
}
