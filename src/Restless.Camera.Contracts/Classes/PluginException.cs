using System;

namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Represents a plugin exception
    /// </summary>
    public class PluginException : Exception
    {
        #region Properties
        /// <summary>
        /// Gets the name of the method where the exception occurred.
        /// </summary>
        public string Method
        {
            get;
        }

        /// <summary>
        /// Gets the request uri associated with the exception
        /// </summary>
        public string RequestUri
        {
            get;
        }
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginException"/> class
        /// </summary>
        /// <param name="method">The name of the method where the exception occured.</param>
        /// <param name="requestUri">The request uri associated with the exception.</param>
        /// <param name="original">The original exception</param>
        public PluginException(string method, string requestUri, Exception original) : base(original?.Message, original)
        {
            Method = method;
            RequestUri = requestUri;
        }
        #endregion

        /************************************************************************/

        #region Methods
        /// <summary>
        /// Gets a string representation of this object.
        /// </summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return $"{nameof(PluginException)}: {Method} {RequestUri} {Message}";
        }
        #endregion
    }
}