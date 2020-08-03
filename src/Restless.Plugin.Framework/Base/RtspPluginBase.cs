using Restless.Camera.Contracts;
using RtspClientSharp;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RtspRawVideo = RtspClientSharp.RawFrames.Video;
using ContractsRawVideo = Restless.Camera.Contracts.RawFrames.Video;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// EXtends <see cref="HttpPluginBase"/> to provide support for plugins that use Rtsp. This class must be inherited.
    /// </summary>
    /// <remarks>
    /// This class provides a base class for camera plugins that use Rtsp and Http. 
    /// </remarks>
    public abstract class RtspPluginBase : HttpPluginBase
    {
        #region Properties
        private CancellationTokenSource rtspTokenSource;
        private CancellationToken rtspToken;
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

        public override void StartVideo()
        {
            /* Only two choices, but using switch in case of future expansion */
            switch (VideoStreams[VideoStreamIndex].Protocol)
            {
                case TransportProtocol.Rtsp:
                    StartVideoRtsp();
                    break;
                case TransportProtocol.Http:
                    base.StartVideo();
                    break;
            }
        }

        public override Task StopVideoAsync()
        {
            if (rtspTokenSource != null)
            {
                rtspTokenSource.Cancel();
            }
            return base.StopVideoAsync();
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private async void StartVideoRtsp()
        {
            string requestUri = string.Empty;
            try
            {
                rtspTokenSource = new CancellationTokenSource();
                rtspToken = rtspTokenSource.Token;
                
                requestUri = $"{GetDeviceRoot(TransportProtocol.Rtsp)}/{VideoStreams[VideoStreamIndex].Path}";

                var serverUri = new Uri(requestUri);
                var credentials = new NetworkCredential(Parms.UserId, Parms.Password);
                var connectionParms = new RtspClientSharp.ConnectionParameters(serverUri, credentials)
                {
                    RtpTransport = RtpTransportProtocol.TCP,
                    ConnectTimeout = TimeSpan.FromSeconds(10),
                };

                using (var rtspClient = new RtspClient(connectionParms))
                {
                    rtspClient.FrameReceived += RtspFrameRecieved;
                    await rtspClient.ConnectAsync(rtspToken);
                    await rtspClient.ReceiveAsync(rtspToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                OnPluginException(new PluginException(nameof(StartVideo), requestUri, ex));
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
