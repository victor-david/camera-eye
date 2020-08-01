using System;

namespace Restless.Camera.Contracts.RawFrames.Video
{
    /// <summary>
    /// Represents a raw video frame. This class must be inherited.
    /// </summary>
    public abstract class RawVideoFrame : RawFrame
    {
        /// <summary>
        /// Gets the frame type (video)
        /// </summary>
        public override FrameType Type => FrameType.Video;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawVideoFrame"/> class.
        /// </summary>
        /// <param name="timestamp">Frame timestamp.</param>
        /// <param name="segment">Frame segment.</param>
        protected RawVideoFrame(DateTime timestamp, ArraySegment<byte> segment) : base(timestamp, segment)
        {
        }
    }
}