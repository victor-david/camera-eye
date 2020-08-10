using Restless.Camera.Contracts;
using Restless.Camera.Contracts.RawFrames.Video;
using System;
using System.Threading.Tasks;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Provides a base class for plugins that use an http MJPEG video stream. This class must be inherited.
    /// </summary>
    public abstract class MjpegPluginBase : HttpPluginBase
    {
        #region Private
        private MultiPartReader reader;
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
        public virtual async void StartVideo()
        {
            string requestUri = string.Empty;
            try 
            {
                if (this is ICameraPlugin video)
                {
                    requestUri = $"{GetDeviceRoot(TransportProtocol.Http)}/{VideoStreams[VideoStreamIndex].Path}";
                    reader = new MultiPartReader(new MultiPartStream(await Client.GetStreamAsync(requestUri).ConfigureAwait(false)));
                    reader.PartReady += MultiPartReady; 
                    reader.StartProcessing();
                }
            }
            catch (Exception ex)
            {
                OnPluginException(new PluginException(requestUri, ex));
            }
        }

        /// <summary>
        /// Stops the video.
        /// </summary>
        public virtual async Task StopVideoAsync()
        {
            if (reader != null) await reader.StopProcessingAsync();
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private void MultiPartReady(object sender, byte[] data)
        {
            OnVideoFrameReceived(new RawJpegFrame(DateTime.Now, new ArraySegment<byte>(data)));
        }
        #endregion
    }
}