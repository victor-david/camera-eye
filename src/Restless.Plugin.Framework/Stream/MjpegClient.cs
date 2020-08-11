using Restless.Camera.Contracts.RawFrames.Video;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Provides services for MJPEG video stream.
    /// </summary>
    public sealed class MjpegClient : IDisposable
    {
        #region Private
        private bool isDisposed;
        private readonly HttpClient httpClient;
        private readonly Uri uri;
        private Stream clientStream;
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MjpegClient"/> class.
        /// </summary>
        /// <param name="httpClient">The underlying http client used to provide the transport.</param>
        public MjpegClient(HttpClient httpClient, Uri uri)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }
        #endregion

        /************************************************************************/

        #region Events
        /// <summary>
        /// Occurs when a new frame is received.
        /// </summary>
        public event EventHandler<RawJpegFrame> FrameReceived;
        #endregion

        /************************************************************************/

        #region IDisposable
        /// <summary>
        /// Disposes.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                clientStream?.Dispose();
            }

            isDisposed = true;
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Asynchonously begins receiving data.
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <returns>The task</returns>
        public async Task ReceiveAsync(CancellationToken token)
        {
            try
            {
                clientStream = await httpClient.GetStreamAsync(uri).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                
                using (var mjpegStream = new MjpegStreamReader(clientStream))
                {
                    mjpegStream.FrameReceived += MpStreamFrameReceived;
                    await mjpegStream.ReceiveAsync(token).ConfigureAwait(false);
                }
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("401"))
            {
                throw new InvalidCredentialException(ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private void MpStreamFrameReceived(object sender, RawJpegFrame e)
        {
            FrameReceived?.Invoke(this, e);
        }
        #endregion
    }
}