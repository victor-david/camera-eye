using Restless.Camera.Contracts.RawFrames.Audio;
using Restless.Camera.Contracts.RawFrames.Video;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Restless.Camera.Contracts
{
    /// <summary>
    /// Defines properties and methods that all camera plugins must implement
    /// </summary>
    public interface ICameraPlugin : IDisposable
    {
        /// <summary>
        /// Gets or sets the orientation of the video.
        /// </summary>
        Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets which video stream to use, i.e. the index into <see cref="VideoStreams"/>
        /// </summary>
        int VideoStreamIndex { get; set; }

        /// <summary>
        /// Gets a list of available video stream descriptors.
        /// </summary>
        IList<VideoStreamDescriptor> VideoStreams { get; }

        /// <summary>
        /// Instructs the plugin to start streaming video
        /// </summary>
        void StartVideo();

        /// <summary>
        /// Instructs the plugin to stop streaming video.
        /// </summary>
        Task StopVideoAsync();

        /// <summary>
        /// Raised when a video frame has been received.
        /// </summary>
        event EventHandler<RawVideoFrame> VideoFrameReceived;

        /// <summary>
        /// Raised when an audio frame has been received.
        /// </summary>
        event EventHandler<RawAudioFrame> AudioFrameReceived;

        /// <summary>
        /// Raised when a plugin exception occurs.
        /// </summary>
        event EventHandler<PluginException> PluginException;
    }
}