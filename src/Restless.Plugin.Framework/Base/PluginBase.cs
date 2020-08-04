using Restless.Camera.Contracts;
using Restless.Camera.Contracts.RawFrames.Audio;
using Restless.Camera.Contracts.RawFrames.Video;
using System;
using System.Collections.Generic;

namespace Restless.Plugin.Framework
{
    /// <summary>
    /// Represents the base class for camera plugins. This class must be inherited.
    /// </summary>
    public abstract class PluginBase
    {
        #region Properties
        /// <summary>
        /// Gets the connection parms.
        /// </summary>
        public ConnectionParameters Parms
        {
            get;
        }

        /// <summary>
        /// Gets a list of available video streams.
        /// </summary>
        public IList<VideoStreamDescriptor> VideoStreams { get; }

        /// <summary>
        /// Gets or sets the index into <see cref="VideoStreams"/>.
        /// The default value is zero. If you have multiple streams,
        /// override this property to clamp an incoming value as needed.
        /// </summary>
        public virtual int VideoStreamIndex { get; set; }

        /// <summary>
        /// Gets a boolean value that indicates if this instance is disposed.
        /// </summary>
        protected bool IsDisposed
        {
            get;
            private set;
        }
        #endregion

        /************************************************************************/

        #region Events
        /// <summary>
        /// Raised when a video frame is received.
        /// </summary>
        public event EventHandler<RawVideoFrame> VideoFrameReceived;

        /// <summary>
        /// Raised when an audio frame is received.
        /// </summary>
        public event EventHandler<RawAudioFrame> AudioFrameReceived;

        /// <summary>
        /// Raised when a plugin exception occurs.
        /// </summary>
        public event EventHandler<PluginException> PluginException;
        #endregion

        /************************************************************************/

        #region Constructor
        protected PluginBase(ConnectionParameters parms)
        {
            Parms = parms ?? throw new ArgumentNullException(nameof(parms));
            VideoStreams = new List<VideoStreamDescriptor>();
            VideoStreamIndex = 0;
        }
        #endregion

        /************************************************************************/

        #region IDisposable
        /// <summary>
        /// Dispoases.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets <see cref="IsDisposed"/> to true. If overriding, always call the base method.
        /// </summary>
        /// <param name="disposing">true if disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;
        }
        #endregion

        /************************************************************************/

        #region Methods
        /// <summary>
        /// Gets the device root for the specified transport protocol.
        /// </summary>
        /// <param name="protocol">The transport protocol.</param>
        /// <returns>A string such as "http://192.168.1.50:80" or rtsp://192.168.1.50</returns>
        protected string GetDeviceRoot(TransportProtocol protocol)
        {
            return protocol switch
            {
                TransportProtocol.Http => $"http://{Parms.IpAddress}:{Parms.Port}",
                TransportProtocol.Rtsp => $"rtsp://{Parms.IpAddress}:{Parms.Port}",
                _ => throw new NotSupportedException("Protocol not supported"),
            };
        }

        /// <summary>
        /// Raises the <see cref="VideoFrameReceived"/> event.
        /// </summary>
        /// <param name="e">The frame</param>
        protected virtual void OnVideoFrameReceived(RawVideoFrame e)
        {
            VideoFrameReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="AudioFrameReceived"/> event.
        /// </summary>
        /// <param name="e">The frame</param>
        protected virtual void OnAudioFrameReceived(RawAudioFrame e)
        {
            AudioFrameReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="PluginException"/> event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPluginException(PluginException e)
        {
            PluginException?.Invoke(this, e);
        }
        #endregion
    }
}