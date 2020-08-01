namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Provides connection parameters for a <see cref="ICameraPlugin"/>
    /// </summary>
    public class ConnectionParameters
    {
        #region Properties
        /// <summary>
        /// Gets the ip address of the camera.
        /// </summary>
        public string IpAddress { get; }

        /// <summary>
        /// Gets the port
        /// </summary>
        public long Port { get; }

        /// <summary>
        /// Gets or sets the user id needed to connect.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the password needed to connect.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the timeout in seconds. Default is 10 seconds.
        /// </summary>
        public int Timeout { get; set; } = 10;

        /// <summary>
        /// Gets a boolean value that indicates if a user id is present.
        /// </summary>
        public bool HasUserId => !string.IsNullOrEmpty(UserId);

        /// <summary>
        /// Gets a boolean value that indicates if a password is present.
        /// </summary>
        public bool HasPassword => !string.IsNullOrEmpty(Password);
        #endregion

        /************************************************************************/

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionParameters"/> class.
        /// </summary>
        /// <param name="ipAddress">The ip address of the camera.</param>
        /// <param name="port">The port.</param>
        /// <param name="userId">The user id, or null if none.</param>
        /// <param name="password">The password, or null if none.</param>
        public ConnectionParameters(string ipAddress, long port, string userId, string password)
        {
            IpAddress = ipAddress;
            Port = port;
            UserId = userId;
            Password = password;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionParameters"/> class.
        /// </summary>
        /// <param name="ipAddress">The ip address of the camera.</param>
        /// <param name="port">The port.</param>
        public ConnectionParameters(string ipAddress, long port) : this(ipAddress, port, null, null)
        {
        }
        #endregion
    }
}