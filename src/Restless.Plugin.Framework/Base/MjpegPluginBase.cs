using Restless.Camera.Contracts;
using Restless.Camera.Contracts.RawFrames.Video;
using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Provides a base class for plugins that use an http MJPEG video stream. This class must be inherited.
    /// </summary>
    public abstract class MjpegPluginBase : HttpPluginBase
    {
        #region Private
        private CancellationTokenSource tokenSource;
        private CancellationToken token;
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MjpegPluginBase"/> class.
        /// </summary>
        /// <param name="parms">The connection parameters</param>
        protected MjpegPluginBase(ConnectionParameters parms) : base(parms)
        {
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Starts the video.
        /// </summary>
        public virtual void StartVideo()
        {
            if (this is ICameraPlugin)
            {
                StartVideoMjpeg();
            }
        }

        /// <summary>
        /// Stops the video.
        /// </summary>
        public virtual async Task StopVideoAsync()
        {
            await Task.Run(() => tokenSource?.Cancel());
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Starts an MJPEG video stream
        /// </summary>
        protected async void StartVideoMjpeg()
        {
            int tryCount = 0;
            string requestUri = string.Empty;

            while (tryCount < Parms.ConnectionAttempts)
            {
                try
                {
                    tokenSource = new CancellationTokenSource();
                    token = tokenSource.Token;

                    requestUri = $"{GetDeviceRoot(TransportProtocol.Http)}/{VideoStreams[VideoStreamIndex].Path}";

                    /* if retrying, wait before another attempt */
                    if (tryCount > 0)
                    {
                        await Task.Delay(Parms.RetryWaitTime, token);
                    }

                    Uri serverUri = new Uri(requestUri);

                    using (var mjpegClient = new MjpegClient(Client, serverUri))
                    {
                        mjpegClient.FrameReceived += MjpegClientFrameReceived;
                        await mjpegClient.ReceiveAsync(token);
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
        #endregion

        /************************************************************************/

        #region Private methods
        private void MjpegClientFrameReceived(object sender, RawJpegFrame e)
        {
            OnVideoFrameReceived(e);
        }
        #endregion
    }
}