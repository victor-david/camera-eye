using System;

namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Provides connection parameters for a <see cref="ICameraPlugin"/>
    /// </summary>
    public class ConnectionParameters
    {
        #region Private
        private int timeout;
        private int connectionAttempts;
        private int retryWaitTime;
        #endregion

        /************************************************************************/

        #region Public constants
        /// <summary>
        /// Minimum value for <see cref="Timeout"/>.
        /// </summary>
        public const int MinTimeout = 5000;

        /// <summary>
        /// Maximum value for <see cref="Timeout"/>.
        /// </summary>
        public const int MaxTimeout = 60000;

        /// <summary>
        /// Default value for <see cref="Timeout"/>.
        /// </summary>
        public const int DefaultTimeout = 10000;

        /// <summary>
        /// Minimum value for <see cref="ConnectionAttempts"/>.
        /// </summary>
        public const int MinConnectionAttempts = 1;

        /// <summary>
        /// Maximum value for <see cref="ConnectionAttempts"/>.
        /// </summary>
        public const int MaxConnectionAttempts = 32;

        /// <summary>
        /// Default value for <see cref="ConnectionAttempts"/>.
        /// </summary>
        public const int DefaultConnectionAttempts = 6;

        /// <summary>
        /// Minimum value for <see cref="RetryWaitTime"/>.
        /// </summary>
        public const int MinRetryWaitTime = 3000;

        /// <summary>
        /// Maximum value for <see cref="RetryWaitTime"/>.
        /// </summary>
        public const int MaxRetryWaitTime = 30000;

        /// <summary>
        /// Default value for <see cref="RetryWaitTime"/>.
        /// </summary>
        public const int DefaultRetryWaitTime = 7500;
        #endregion

        /************************************************************************/

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
        /// Gets or sets the timeout in milliseconds. Default is 10000, i.e. 10 seconds.
        /// This value is clamped between <see cref="MinTimeout"/> and <see cref="MaxTimeout"/>, inclusive.
        /// </summary>
        public int Timeout
        {
            get => timeout;
            set => timeout = Math.Max(Math.Min(value, MaxTimeout), MinTimeout);
        }

        /// <summary>
        /// Gets or sets the number of connection attempts when obtaining the video stream.
        /// This value is clamped between <see cref="MinConnectionAttempts"/> and <see cref="MaxConnectionAttempts"/>, inclusive.
        /// Default is 6.
        /// </summary>
        public int ConnectionAttempts
        {
            get => connectionAttempts;
            set => connectionAttempts = Math.Max(Math.Min(value, MaxConnectionAttempts), MinConnectionAttempts);
        }

        /// <summary>
        /// Gets or sets the time in milliseconds to wait after a failed attempt to connect to the video stream before trying again.
        /// This value is clamped between <see cref="MinRetryWaitTime"/> and <see cref="MaxRetryWaitTime"/>, inclusive.
        /// Default is 7500.
        /// </summary>
        public int RetryWaitTime
        {
            get => retryWaitTime;
            set => retryWaitTime = Math.Max(Math.Min(value, MaxRetryWaitTime), MinRetryWaitTime);
        }

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
            Timeout = DefaultTimeout;
            ConnectionAttempts = DefaultConnectionAttempts;
            RetryWaitTime = DefaultRetryWaitTime;
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