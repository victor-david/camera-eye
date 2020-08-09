using Restless.Camera.Contracts;
using Restless.Camera.Contracts.RawFrames.Video;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Provides a base class for plugins that use http. This class must be inherited.
    /// </summary>
    /// <remarks>
    /// This class provides a base class for camera plugins that use Http. 
    /// </remarks>
    public abstract class HttpPluginBase : PluginBase
    {
        #region Private
        private MultiPartReader reader;
        #endregion

        /************************************************************************/

        #region Properties
        /// <summary>
        /// Gets the http client instance.
        /// </summary>
        protected HttpClient Client
        {
            get;
            private set;
        }
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpPluginBase"/> class.
        /// </summary>
        /// <param name="parms">The connection parameters</param>
        protected HttpPluginBase(ConnectionParameters parms) : base(parms)
        {
            var clientHandler = new HttpClientHandler();

            Client = new HttpClient(new HttpTimeoutHandler()
            {
                DefaultTimeout = TimeSpan.FromSeconds(Math.Max(Math.Min(parms.Timeout, 60), 5)),
                InnerHandler = clientHandler
            })
            {
                Timeout = Timeout.InfiniteTimeSpan,
            };

            if (parms.HasUserId)
            {
                clientHandler.Credentials = new NetworkCredential(parms.UserId, parms.Password);
            }
        }
        #endregion

        /************************************************************************/

        #region IDisposable
        /// <summary>
        /// Called when disposing. Disposes of http client.
        /// </summary>
        /// <param name="disposing">true when disposing.</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                // free managed resources
                if (Client != null)
                {
                    Client.Dispose();
                    Client = null;
                }
            }
            base.Dispose(disposing);
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
                OnPluginException(new PluginException(nameof(StartVideo), requestUri, ex));
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

        #region Protected methods
        /// <summary>
        /// Performs a client request using GET.
        /// </summary>
        /// <param name="requestUri">The request uri</param>
        /// <returns>The body of the response, or null if error.</returns>
        protected async Task<string> PerformClientGetAsync(string requestUri)
        {
            try
            {
                HttpResponseMessage response = await Client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
                //OnHttpResponse(response);
            }
            catch (Exception ex)
            {
                OnPluginException(new PluginException(nameof(PerformClientGetAsync), requestUri, ex));
                return null;
            }
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