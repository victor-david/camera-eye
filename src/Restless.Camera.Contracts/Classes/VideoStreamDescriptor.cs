namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Describes a video stream for a camera plugin.
    /// </summary>
    public class VideoStreamDescriptor
    {
        #region Properties
        /// <summary>
        /// Gets the transport protocol.
        /// </summary>
        public TransportProtocol Protocol { get; }

        /// <summary>
        /// Gets the path from the root.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the friendly name / description.
        /// </summary>
        public string FriendlyName { get; }
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoStreamDescriptor"/> class.
        /// </summary>
        /// <param name="protocol">The protocol used by the video stream.</param>
        /// <param name="path">The path to the video stream.</param>
        /// <param name="friendlyName">Friendly name / description</param>
        public VideoStreamDescriptor(TransportProtocol protocol, string path, string friendlyName)
        {
            Protocol = protocol;
            Path = path;
            FriendlyName = friendlyName;
        }
        #endregion
    }
}
