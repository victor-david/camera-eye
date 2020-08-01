using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Represents a delegating handler to handle timeout.
    /// https://thomaslevesque.com/2018/02/25/better-timeout-handling-with-httpclient/
    /// </summary>
    public class HttpTimeoutHandler : DelegatingHandler
    {
        #region Public properties
        /// <summary>
        /// Gets or sets the default timeout. 60 seconds.
        /// </summary>
        public TimeSpan DefaultTimeout
        {
            get;
            set;
        } = TimeSpan.FromSeconds(60);
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Sends the specified http message.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancelation token.</param>
        /// <returns>An <see cref="HttpResponseMessage"/> object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:Use simple 'using' statement", Justification = "Like it this way")]
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var tokenSource = GetCancellationTokenSource(request, cancellationToken))
            {
                try
                {
                    return await base.SendAsync(request, tokenSource?.Token ?? cancellationToken);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }
            }
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private CancellationTokenSource GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            TimeSpan timeout = request.GetTimeout() ?? DefaultTimeout;

            if (timeout == Timeout.InfiniteTimeSpan)
            {
                // No need to create a CTS if there's no timeout
                return null;
            }
            else
            {
                var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                tokenSource.CancelAfter(timeout);
                return tokenSource;
            }
        }
        #endregion
    }
}