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
        /// Gets the message of this exception.
        /// </summary>
        public override string Message => $"{RequestUri}{Environment.NewLine}{InnerException.Message}";

        /// <summary>
        /// Gets the request uri associated with the exception.
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
        /// <param name="requestUri">The request uri associated with the exception.</param>
        /// <param name="original">The original exception. Becomes the inner exception of this</param>
        public PluginException(string requestUri, Exception original) : base(original.Message, original)
        {
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
            return $"{nameof(PluginException)}: {Message}";
        }
        #endregion
    }
}