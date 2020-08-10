using Restless.Camera.Contracts;
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
                DefaultTimeout = TimeSpan.FromMilliseconds(Parms.Timeout),
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
                //OnHttpResponse(response);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                OnPluginException(new PluginException(requestUri, ex));
                return null;
            }
        }
        #endregion
    }
}