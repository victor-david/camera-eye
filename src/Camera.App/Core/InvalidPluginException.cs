using System;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Represents the exception that is thrown when the camera plugin is invalid.
    /// </summary>
    public class InvalidPluginException : Exception
    {
        public InvalidPluginException(string message) : base(message)
        {
        }
    }
}
