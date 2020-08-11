using Restless.Camera.Contracts;
using RtspClientSharp;
using System;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using ContractsRawVideo = Restless.Camera.Contracts.RawFrames.Video;
using RtspRawVideo = RtspClientSharp.RawFrames.Video;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Extends <see cref="MjpegPluginBase"/> to provide support for plugins that use Rtsp. This class must be inherited.
    /// </summary>
    /// <remarks>
    /// This class provides a base class for camera plugins that use Rtsp and Mjpeg video. 
    /// </remarks>
    public abstract class RtspPluginBase : MjpegPluginBase
    {
        #region Private
        private CancellationTokenSource tokenSource;
        private CancellationToken token;
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPluginBase"/> class.
        /// </summary>
        /// <param name="parms">The connection parameters</param>
        protected RtspPluginBase(Camera.Contracts.ConnectionParameters parms) : base(parms)
        {
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Starts the video.
        /// </summary>
        public override void StartVideo()
        {
            if (this is ICameraPlugin)
            {
                /* Only two choices, but using switch in case of future expansion */
                switch (VideoStreams[VideoStreamIndex].Protocol)
                {
                    case TransportProtocol.Rtsp:
                        StartVideoRtsp();
                        break;
                    case TransportProtocol.Http:
                        base.StartVideoMjpeg();
                        break;
                }
            }
        }

        /// <summary>
        /// Stops the video.
        /// </summary>
        /// <returns>A Task that represents the work.</returns>
        public override async Task StopVideoAsync()
        {
            await Task.Run(() => tokenSource?.Cancel());
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private async void StartVideoRtsp()
        {
            int tryCount = 0;
            string requestUri = string.Empty;

            while (tryCount < Parms.ConnectionAttempts)
            {
                try
                {
                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;

                    requestUri = $"{GetDeviceRoot(TransportProtocol.Rtsp)}/{VideoStreams[VideoStreamIndex].Path}";

                    /* if retrying, wait before another attempt */
                    if (tryCount > 0)
                    {
                        await Task.Delay(Parms.RetryWaitTime, token);
                    }

                    var serverUri = new Uri(requestUri);
                    var credentials = new NetworkCredential(Parms.UserId, Parms.Password);
                    var connectionParms = new RtspClientSharp.ConnectionParameters(serverUri, credentials)
                    {
                        RtpTransport = RtpTransportProtocol.TCP,
                        ConnectTimeout = TimeSpan.FromMilliseconds(Parms.Timeout),
                    };

                    using (var rtspClient = new RtspClient(connectionParms))
                    {
                        rtspClient.FrameReceived += RtspFrameRecieved;
                        await rtspClient.ConnectAsync(token).ConfigureAwait(false);
                        /* Once connected, reset the try count. If there's a network problem
                         * while receiving, we get another full set of retries
                         */
                        tryCount = 0;
                        await rtspClient.ReceiveAsync(token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    /* when canceled, no more attempts */
                    tryCount = Parms.ConnectionAttempts;
                }

                catch (InvalidCredentialException ex)
                {
                    /* bad credentials, no more attempts */
                    OnPluginException(new PluginException(requestUri, ex));
                    tryCount = Parms.ConnectionAttempts;
                }
                catch (Exception ex)
                {
                    OnPluginException(new PluginException(requestUri, ex));
                    tryCount++;
                }
            }
        }

        private void RtspFrameRecieved(object sender, RtspClientSharp.RawFrames.RawFrame frame)
        {
            if (frame.Type == RtspClientSharp.RawFrames.FrameType.Video)
            {
                ContractsRawVideo.RawVideoFrame rawFrame = ToContractsRawVideoFrame(frame);
                if (rawFrame != null)
                {
                    OnVideoFrameReceived(rawFrame);
                }
            }
        }

        private ContractsRawVideo.RawVideoFrame ToContractsRawVideoFrame(RtspClientSharp.RawFrames.RawFrame frame)
        {
            if (frame is RtspRawVideo.RawH264IFrame f1)
            {
                return new ContractsRawVideo.RawH264IFrame(f1.Timestamp, f1.FrameSegment, f1.SpsPpsSegment);
            }
            if (frame is RtspRawVideo.RawH264PFrame f2)
            {
                return new ContractsRawVideo.RawH264PFrame(f2.Timestamp, f2.FrameSegment);
            }
            return null;
        }
        #endregion
    }
}
