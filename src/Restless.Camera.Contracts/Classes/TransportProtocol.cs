namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Describes the transport protocols a plugin may use.
    /// </summary>
    public enum TransportProtocol
    {
        /// <summary>
        /// Transport is handled via http
        /// </summary>
        Http,
        /// <summary>
        /// Transport is handled via rtsp
        /// </summary>
        Rtsp,
    }
}
