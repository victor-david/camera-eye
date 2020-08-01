using System;
using System.Net.Http;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Provides extension methods for network operations.
    /// </summary>
    public static class NetworkExtensions
    {
        #region Public fields
        /// <summary>
        /// Defines the dictionary key used to hold the timeout value.
        /// </summary>
        public const string TimeoutPropertyKey = "TimeoutPropertyKey";
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Sets the timeout for the request
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="timeout">The timeout to set for the request.</param>
        public static void SetTimeout(this HttpRequestMessage request, TimeSpan timeout)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Properties[TimeoutPropertyKey] = timeout;
        }

        /// <summary>
        /// Gets the timeout for the request, or null if not set.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A TimeSpan, or null if no timeout set.</returns>
        public static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request.Properties.TryGetValue(TimeoutPropertyKey, out var value) && value is TimeSpan timeout)
            {
                return timeout;
            }
            return null;
        }
        #endregion
    }
}
